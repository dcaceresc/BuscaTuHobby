using System.Text.RegularExpressions;
using Application.Common.Interfaces;

namespace Application.Common.Ingestion;

/// <summary>
/// La única regla que decide si una URL es la ficha de producto de una tienda. Vive fuera del conector y del
/// descubrimiento porque los dos tienen que responder lo mismo: mientras el recorrido encolaba entradas que el
/// conector rechazaba, el presupuesto de la ejecución gastaba peticiones en fichas que nadie podía leer.
/// Es una función pura: depende sólo de la tienda y de la URL, nunca de la red ni de la base.
/// </summary>
public static class ProductSheetUrl
{
    /// <summary>
    /// Plataformas cuya forma de ficha se conoce hoy. Se guardan aquí y no en el conector para que la regla y la
    /// clave que la activa sean el mismo literal: el descubrimiento no necesita la lista de conectores registrados.
    /// </summary>
    public const string ShopifyConnectorKey = "shopify";

    /// <summary>
    /// Tiendas que publican la ficha con datos estructurados. No tienen una ruta de producto fija: el catálogo cuelga
    /// de rutas distintas por tienda (`/producto/...`, una URL plana, una anidada), así que la forma declarada es
    /// "mismo host sobre https, cualquier ruta".
    /// </summary>
    public const string JsonLdConnectorKey = "json-ld";

    private const int MaximumUrlLength = 8192;
    private const string ShopifySheetSuffix = ".js";

    // La ruta se valida tal como se escribió: Uri normaliza los segmentos con punto y las barras invertidas, así que
    // aceptar su versión normalizada dejaría pasar rutas que el catálogo no publica.
    private static readonly Regex ShopifyProductPath = new(
        @"\A/(?:collections/[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*/)?products/([a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*)/?\z",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Responde si <paramref name="url"/> es una ficha de producto de la tienda y devuelve su identificador de
    /// catálogo. Una tienda cuya plataforma no declara forma de ficha no acepta ninguna URL: es preferible no
    /// encolar nada a encolar lo que su conector va a rechazar.
    /// </summary>
    public static bool TryGetHandle(StoreContext store, string url, out string handle)
    {
        ArgumentNullException.ThrowIfNull(store);
        handle = string.Empty;

        if (!IsShopifyStore(store) || !TryParseStoreUrl(store, url, out _, out var rawPath))
            return false;

        var match = ShopifyProductPath.Match(rawPath);
        if (!match.Success) return false;
        handle = match.Groups[1].Value;
        return true;
    }

    /// <summary>
    /// Responde si <paramref name="url"/> es una ficha de la tienda según la forma que declara su plataforma. Es la
    /// pregunta que hace el descubrimiento: no necesita el identificador interno, sólo saber si merece una petición.
    /// </summary>
    public static bool IsProductSheet(StoreContext store, string url)
    {
        ArgumentNullException.ThrowIfNull(store);
        return IsJsonLdStore(store) ? TryGetJsonLdSheetUrl(store, url, out _) : TryGetHandle(store, url, out _);
    }

    /// <summary>
    /// Forma de ficha de las tiendas con JSON-LD: mismo host sobre https y cualquier ruta, porque la tienda publica su
    /// producto en la ruta que quiera. La raíz queda fuera: es el escaparate de la tienda, nunca la ficha de un
    /// producto. Devuelve la URL canónica sin consulta ni fragmento, que es la que se pide y se guarda como origen.
    /// </summary>
    public static bool TryGetJsonLdSheetUrl(StoreContext store, string url, out string sheetUrl)
    {
        ArgumentNullException.ThrowIfNull(store);
        sheetUrl = string.Empty;

        if (!IsJsonLdStore(store) || !TryParseStoreUrl(store, url, out var uri, out var rawPath))
            return false;

        // La ruta se compara con la normalizada para que "..", "\\" y los espacios no conviertan la lectura en una
        // petición a otra ruta que la que se validó contra robots.txt.
        if (rawPath is "" or "/" || !string.Equals(rawPath, uri.AbsolutePath, StringComparison.Ordinal))
            return false;

        sheetUrl = $"https://{store.RequestHost}{rawPath}";
        return true;
    }

    /// <summary>
    /// Responde si <paramref name="url"/> es una lectura de ficha que el conector puede interpretar. La forma de la
    /// ficha —y no sólo el host— tiene que valer también para el destino de una redirección: si la tienda manda la
    /// lectura a otra ruta, se estaría leyendo algo que nadie validó como ficha de esta tienda.
    /// </summary>
    public static bool IsSheetRead(StoreContext store, string url)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (IsJsonLdStore(store)) return TryGetJsonLdSheetUrl(store, url, out _);
        if (!IsShopifyStore(store)) return false;

        // El conector Shopify lee la ficha por el sufijo .js del endpoint público: el destino sigue siendo la misma
        // ficha sólo si conserva ese sufijo y una ruta de producto del propio host.
        return url.EndsWith(ShopifySheetSuffix, StringComparison.Ordinal) &&
            TryGetHandle(store, url[..^ShopifySheetSuffix.Length], out _);
    }

    public static bool IsJsonLdStore(StoreContext store) =>
        string.Equals(store.ConnectorKey, JsonLdConnectorKey, StringComparison.OrdinalIgnoreCase);

    private static bool IsShopifyStore(StoreContext store) =>
        string.Equals(store.ConnectorKey, ShopifyConnectorKey, StringComparison.OrdinalIgnoreCase);

    // Host, esquema y puerto son comunes a todas las formas: ninguna ficha de ninguna tienda sale de su host por http.
    private static bool TryParseStoreUrl(StoreContext store, string url, out Uri uri, out string rawPath)
    {
        uri = null!;
        rawPath = string.Empty;
        if (string.IsNullOrWhiteSpace(url) || url.Length > MaximumUrlLength ||
            !Uri.TryCreate(url, UriKind.Absolute, out var candidate) ||
            candidate.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(StoreContext.NormalizeHost(candidate.Host), store.Host, StringComparison.Ordinal) ||
            !candidate.IsDefaultPort || candidate.UserInfo.Length != 0)
            return false;

        uri = candidate;
        rawPath = RawPath(url);
        return true;
    }

    // Desde el primer '/' después del esquema hasta '?' o '#': la ruta que realmente se escribió.
    private static string RawPath(string url)
    {
        var scheme = url.IndexOf("://", StringComparison.Ordinal);
        var start = scheme < 0 ? -1 : url.IndexOf('/', scheme + 3);
        if (start < 0) return string.Empty;
        var end = url.IndexOfAny(['?', '#'], start);
        return end < 0 ? url[start..] : url[start..end];
    }
}
