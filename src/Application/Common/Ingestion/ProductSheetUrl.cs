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
    /// Plataforma cuya forma de ficha se conoce hoy. Se guarda aquí y no en el conector para que la regla y la
    /// clave que la activa sean el mismo literal: el descubrimiento no necesita la lista de conectores registrados.
    /// </summary>
    public const string ShopifyConnectorKey = "shopify";

    private const int MaximumUrlLength = 8192;

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

        if (!IsKnownPlatform(store) || string.IsNullOrWhiteSpace(url) || url.Length > MaximumUrlLength ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(StoreContext.NormalizeHost(uri.Host), store.Host, StringComparison.Ordinal) ||
            !uri.IsDefaultPort || uri.UserInfo.Length != 0)
            return false;

        var match = ShopifyProductPath.Match(RawPath(url));
        if (!match.Success) return false;
        handle = match.Groups[1].Value;
        return true;
    }

    private static bool IsKnownPlatform(StoreContext store) =>
        string.Equals(store.ConnectorKey, ShopifyConnectorKey, StringComparison.OrdinalIgnoreCase);

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
