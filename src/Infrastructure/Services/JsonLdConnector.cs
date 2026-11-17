using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Infrastructure.Services;

/// <summary>
/// Conector para las tiendas que publican su ficha con datos estructurados <c>schema.org</c> en bloques
/// <c>application/ld+json</c>. No hay código por tienda: el host, la moneda y el presupuesto salen de la fila y el
/// parser acepta las variantes medidas en las tiendas chilenas (precio entero, decimal o con separadores locales,
/// ofertas como objeto o como arreglo, identificadores y marca ausentes).
/// </summary>
public sealed class JsonLdConnector(HttpClient client, RobotsTxtCache robots, DomainRequestLimiter limiter)
    : IStoreConnector
{
    public const string ConnectorKey = ProductSheetUrl.JsonLdConnectorKey;

    // Una página sin producto se niega: una categoría o un escaparate no puede convertirse en una oferta vacía.
    // El texto se escribe una sola vez para que el mensaje y el motivo registrado sean el mismo.
    public const string NoProductNodeMessage =
        "La página no publica un producto en JSON-LD (schema.org/Product): no se puede leer como ficha.";

    // Un listado publica un nodo por tarjeta. Sin una URL que diga cuál es la ficha, agregarlos sería inventar una
    // oferta que ninguna página declara.
    public const string AmbiguousProductMessage =
        "La página publica varios productos en JSON-LD y ninguno corresponde a la URL pedida: no se puede saber cuál es la ficha.";

    public const string MissingTitleMessage =
        "La página no informa el nombre del producto en JSON-LD.";

    public const string MissingPriceMessage =
        "La ficha no declara un precio válido en JSON-LD.";

    // Una ficha con la plantilla completa ronda los 2 MB; el tope evita descargar una página desmedida.
    private const int MaximumBytes = 4_194_304;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    // Los bloques ld+json no se pueden leer con un parser XML: se extraen del HTML tal como los publica la tienda.
    private static readonly Regex JsonLdBlock = new(
        "<script[^>]*type\\s*=\\s*[\"']application/ld\\+json[\"'][^>]*>(?<json>.*?)</script\\s*>",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant,
        TimeSpan.FromMilliseconds(100));

    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    // Estados de schema.org que significan "no se puede comprar". PreOrder y BackOrder sí se compran, así que la
    // lista de exclusiones es explícita en vez de "todo lo que no sea InStock está agotado".
    private static readonly HashSet<string> NotPurchasable = new(StringComparer.OrdinalIgnoreCase)
    {
        "OutOfStock", "SoldOut", "Discontinued"
    };

    private readonly HttpClient _client = client;
    private readonly RobotsTxtCache _robots = robots;
    private readonly DomainRequestLimiter _limiter = limiter;

    public string Key => ConnectorKey;

    public async Task<IrionProductPreviewDto> LoadProductAsync(
        StoreContext store, string url, CancellationToken cancellationToken) =>
        (await LoadSheetAsync(store, url, cancellationToken)).ToPreview();

    public async Task<StoreProductSheet> LoadSheetAsync(
        StoreContext store, string url, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (!ProductSheetUrl.TryGetJsonLdSheetUrl(store, url, out var sheetUrl))
            throw new StoreOfferUrlException(
                $"Introduce una URL válida de {store.StoreName}: https://{store.Host}/ruta-del-producto");

        var productPath = new Uri(sheetUrl).AbsolutePath;

        // El portal de robots se consulta antes que la ficha: una ruta excluida no llega a pedirse.
        var policy = await _robots.GetAsync(store.RequestHost, cancellationToken);
        if (!policy.Allows(productPath))
            throw new StorePathDisallowedException(store.StoreName, store.Host, productPath);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);
        var token = timeout.Token;
        // El transporte no sigue redirecciones a propósito; el seguidor las sigue con tope y revalida en cada salto el
        // host, la forma de ficha de la plataforma y la ruta nueva contra robots.txt.
        using var response = await StoreRedirectFollower.SendAsync(store, sheetUrl, policy,
            (current, hopToken) => _limiter.SendAsync(store.Host, attempt => SendAsync(current, attempt), hopToken),
            target => ProductSheetUrl.IsSheetRead(store, target), token);
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new StoreRateLimitException(store.StoreName, StoreRetryGuidance.From(response));
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaximumBytes)
            throw new InvalidDataException("La respuesta supera el límite permitido.");

        return BuildSheet(await ReadAsync(response, token), sheetUrl, store);
    }

    private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("text/html");
        // El mismo agente que el conector Shopify: la tienda ve un solo rastreador identificable por plataforma.
        request.Headers.UserAgent.ParseAdd(ShopifyConnector.UserAgent);
        return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private static async Task<string> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaximumBytes)
                throw new InvalidDataException("La respuesta supera el límite permitido.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    // El documento del JSON-LD se mantiene vivo mientras se lee el nodo elegido: JsonElement no es dueño de su
    // memoria y la ficha se construye entera dentro del ámbito que la libera.
    private static StoreProductSheet BuildSheet(string html, string sheetUrl, StoreContext store)
    {
        var nodes = new List<JsonElement>();
        var documents = new List<JsonDocument>();
        try
        {
            foreach (Match block in JsonLdBlock.Matches(html))
            {
                try
                {
                    var document = JsonDocument.Parse(block.Groups["json"].Value, JsonOptions);
                    documents.Add(document);
                    CollectProducts(document.RootElement, nodes);
                }
                catch (JsonException)
                {
                    // Un bloque roto de la plantilla no invalida la página: puede haber otro válido con el producto.
                }
            }

            if (nodes.Count == 0) throw new InvalidDataException(NoProductNodeMessage);
            return Sheet(ChooseProduct(nodes, sheetUrl), sheetUrl, store);
        }
        finally
        {
            foreach (var document in documents) document.Dispose();
        }
    }

    // Un bloque puede ser un nodo suelto, un arreglo o un grafo con @graph: se recorre todo.
    private static void CollectProducts(JsonElement element, List<JsonElement> nodes)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (IsProductNode(element))
                {
                    nodes.Add(element);
                    return;
                }

                foreach (var property in element.EnumerateObject())
                    CollectProducts(property.Value, nodes);
                return;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectProducts(item, nodes);
                return;
        }
    }

    // "Product" y "ProductGroup" se aceptan: una tienda puede agrupar variantes en vez de publicar una ficha por
    // variante, y el grupo declara el precio igual que el producto.
    private static bool IsProductNode(JsonElement node) =>
        node.TryGetProperty("@type", out var type) && TypeNames(type).Any(IsProductType);

    private static IEnumerable<string> TypeNames(JsonElement type) => type.ValueKind switch
    {
        JsonValueKind.String => [type.GetString() ?? string.Empty],
        JsonValueKind.Array => type.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray(),
        _ => []
    };

    // "@type" puede venir como "Product", "schema:Product" o "https://schema.org/Product": el nombre es el último
    // segmento, así que las tres formas valen.
    private static bool IsProductType(string value)
    {
        var name = value.Trim();
        var separator = name.LastIndexOfAny(['/', '#', ':']);
        if (separator >= 0) name = name[(separator + 1)..];
        return name is "Product" or "ProductGroup";
    }

    // Una ficha tiene un solo producto. Un listado trae uno por tarjeta: sólo se acepta cuando exactamente uno
    // declara la URL pedida, y si ninguno lo hace la lectura se niega en vez de sumar precios de productos distintos.
    private static JsonElement ChooseProduct(List<JsonElement> nodes, string sheetUrl)
    {
        if (nodes.Count == 1) return nodes[0];
        var matches = nodes.Where(node => DeclaresUrl(node, sheetUrl)).ToList();
        if (matches.Count == 1) return matches[0];
        throw new InvalidDataException(AmbiguousProductMessage);
    }

    private static bool DeclaresUrl(JsonElement node, string sheetUrl)
    {
        var wanted = new Uri(sheetUrl);
        foreach (var name in new[] { "url", "@id", "mainEntityOfPage" })
        {
            var declared = UrlValue(node, name);
            if (string.IsNullOrWhiteSpace(declared) ||
                !Uri.TryCreate(declared.Trim(), UriKind.Absolute, out var candidate) ||
                candidate.Scheme != Uri.UriSchemeHttps || !candidate.IsDefaultPort)
                continue;

            if (string.Equals(StoreContext.NormalizeHost(candidate.Host),
                    StoreContext.NormalizeHost(wanted.Host), StringComparison.Ordinal) &&
                string.Equals(candidate.AbsolutePath.TrimEnd('/'), wanted.AbsolutePath.TrimEnd('/'),
                    StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static StoreProductSheet Sheet(JsonElement node, string sheetUrl, StoreContext store)
    {
        var title = Text(node, "name") ?? throw new InvalidDataException(MissingTitleMessage);
        var offer = ChosenOffer(node);
        var sku = Identifier(node, "sku");
        var barcode = Identifier(node, "gtin13") ?? Identifier(node, "gtin") ?? Identifier(node, "ean");

        // La moneda de la fila manda; el JSON-LD sólo cubre el caso de una tienda configurada sin moneda, porque el
        // precio publicado y el catálogo tienen que hablar la misma.
        var currency = Currency(store.Currency, offer.Currency);

        // El identificador externo es el SKU cuando existe y, si la ficha no lo informa, la propia URL de la ficha:
        // la oferta necesita algo estable que la distinga dentro de la tienda.
        var externalVariantId = sku ?? sheetUrl;

        return new StoreProductSheet(sheetUrl, title, Vendor(node), currency,
            Text(node, "description") ?? string.Empty,
            // El JSON-LD declara la categoría donde Shopify declara el tipo de producto: la ingesta la usa igual.
            Text(node, "category"), [], Images(node), offer.Available,
            [new IrionVariantPreviewDto(externalVariantId, sku, barcode, offer.Available, offer.Price, null, null)]);
    }

    private sealed record DeclaredOffer(decimal Price, string? Currency, bool Available);

    private static DeclaredOffer ChosenOffer(JsonElement node)
    {
        foreach (var offer in OfferNodes(node))
        {
            if (!TryPrice(offer, out var price)) continue;
            return new DeclaredOffer(price, Text(offer, "priceCurrency"), Available(offer));
        }

        throw new InvalidDataException(MissingPriceMessage);
    }

    // "offers" puede ser un objeto o un arreglo de ofertas; se conserva el orden que publica la tienda.
    private static IReadOnlyList<JsonElement> OfferNodes(JsonElement node)
    {
        if (!node.TryGetProperty("offers", out var offers)) return [];
        return offers.ValueKind switch
        {
            JsonValueKind.Object => [offers],
            JsonValueKind.Array => offers.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.Object)
                .ToArray(),
            _ => []
        };
    }

    // Una ficha con precio y sin disponibilidad declarada se considera disponible: el precio es la señal de que la
    // tienda la vende, y suponer lo contrario escondería todo el catálogo de una tienda que no publica el campo.
    private static bool Available(JsonElement offer)
    {
        var declared = Text(offer, "availability");
        if (declared is null) return true;
        // La disponibilidad es una URL de schema.org: lo que decide es su último segmento, no la URL completa.
        var name = declared.Trim();
        var separator = name.LastIndexOfAny(['/', '#', ':']);
        if (separator >= 0) name = name[(separator + 1)..];
        return !NotPurchasable.Contains(name);
    }

    private static bool TryPrice(JsonElement offer, out decimal price)
    {
        price = 0;
        if (!offer.TryGetProperty("price", out var value)) return false;
        if (value.ValueKind == JsonValueKind.Number) return value.TryGetDecimal(out price) && price > 0;
        if (value.ValueKind != JsonValueKind.String) return false;
        return TryLocalPrice(value.GetString() ?? string.Empty, out price);
    }

    // Los precios medidos vienen como entero, como decimal de JSON ("1990.0") o como texto localizado ("$ 6.390",
    // "6390 CLP"). Se aíslan los dígitos y los separadores y se decide cuál es el decimal: con los dos separadores
    // manda el último, y un único separador seguido de exactamente tres dígitos es de miles, no de centavos.
    private static bool TryLocalPrice(string raw, out decimal price)
    {
        price = 0;
        var text = new string(raw.Where(character => char.IsDigit(character) || character is '.' or ',').ToArray());
        if (text.Length == 0) return false;

        var lastDot = text.LastIndexOf('.');
        var lastComma = text.LastIndexOf(',');
        if (lastDot >= 0 && lastComma >= 0)
        {
            var decimalSeparator = lastDot > lastComma ? '.' : ',';
            var whole = text.Replace(",", string.Empty).Replace(".", string.Empty);
            var cut = whole.Length - (text.Length - 1 - Math.Max(lastDot, lastComma));
            if (cut <= 0) return false;
            text = whole[..cut] + "." + whole[cut..];
        }
        else if (lastDot >= 0 || lastComma >= 0)
        {
            var separator = lastDot >= 0 ? '.' : ',';
            var groups = text.Split(separator);
            text = groups.Length == 2 && groups[1].Length == 3 && groups[0].Length is > 0 and <= 3
                ? groups[0] + groups[1]
                : groups.Length == 2 && groups[1].Length > 0
                    ? groups[0] + "." + groups[1]
                    : string.Concat(groups);
        }

        return decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price) &&
            price > 0;
    }

    // La marca declara el fabricante. Puede venir como texto, como objeto Brand o como arreglo; sin marca el
    // vendedor queda vacío y la identidad del catálogo decide después si con eso basta.
    private static string Vendor(JsonElement node)
    {
        if (!node.TryGetProperty("brand", out var brand)) return string.Empty;
        return brand.ValueKind switch
        {
            JsonValueKind.String => brand.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Object => Text(brand, "name") ?? string.Empty,
            JsonValueKind.Array => brand.EnumerateArray()
                .Select(item => item.ValueKind == JsonValueKind.String
                    ? item.GetString()
                    : item.ValueKind == JsonValueKind.Object ? Text(item, "name") : null)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty,
            _ => string.Empty
        };
    }

    // La imagen puede venir como texto, como arreglo o como ImageObject. Una URL relativa o inválida se descarta:
    // una imagen no decide la identidad del producto, así que no vale la pena rechazar la ficha entera por ella.
    private static IReadOnlyList<string> Images(JsonElement node)
    {
        if (!node.TryGetProperty("image", out var image)) return [];
        var candidates = image.ValueKind == JsonValueKind.Array
            ? image.EnumerateArray().ToArray()
            : [image];
        var images = new List<string>();
        foreach (var candidate in candidates)
        {
            var declared = candidate.ValueKind switch
            {
                JsonValueKind.String => candidate.GetString(),
                JsonValueKind.Object => UrlValue(candidate, "url") ?? UrlValue(candidate, "contentUrl"),
                _ => null
            };
            if (Uri.TryCreate(declared?.Trim(), UriKind.Absolute, out var uri) &&
                uri.Scheme == Uri.UriSchemeHttps && uri.UserInfo.Length == 0)
                images.Add(uri.AbsoluteUri);
        }

        return images;
    }

    // El código de barras se prefiere en el orden que declara el catálogo: gtin13, gtin y por último ean.
    private static string? Identifier(JsonElement node, string name)
    {
        if (!node.TryGetProperty(name, out var value)) return null;
        var text = value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string Currency(string configured, string? declared)
    {
        var store = (configured ?? string.Empty).Trim();
        if (store.Length > 0) return store;
        var published = (declared ?? string.Empty).Trim().ToUpperInvariant();
        return published.Length == 3 && published.All(char.IsLetter)
            ? published
            : StoreConnectorSettings.DefaultCurrency;
    }

    private static string? Text(JsonElement value, string name) =>
        value.TryGetProperty(name, out var field) && field.ValueKind == JsonValueKind.String
            ? Text(field.GetString())
            : null;

    private static string? Text(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // Un campo de URL puede ser texto o un objeto con @id, como en mainEntityOfPage.
    private static string? UrlValue(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var field)) return null;
        return field.ValueKind switch
        {
            JsonValueKind.String => field.GetString(),
            JsonValueKind.Object => Text(field, "@id") ?? Text(field, "url"),
            _ => null
        };
    }
}
