using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Infrastructure.Services;

/// <summary>
/// Conector de la plataforma Shopify. Es el extractor de Irion generalizado: mismo endpoint público de producto
/// (<c>.js</c>) y mismas variantes, pero el host, la moneda y el agente salen de la fila de la tienda. Irion es
/// una fila configurada más, no una clase propia.
/// </summary>
public sealed class ShopifyConnector(HttpClient client, RobotsTxtCache robots, DomainRequestLimiter limiter)
    : IStoreConnector
{
    public const string ConnectorKey = "shopify";

    // Agente estable e identificable: una tienda puede excluir o limitar por nombre en robots.txt.
    public const string UserAgent = "BuscaTuHobby-ProductPreview/1.0";

    private const int MaximumBytes = 1_048_576;
    private const int MaximumUrlLength = 8192;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);
    private static readonly Regex ProductPath = new(
        @"\A/(?:collections/[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*/)?products/([a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*)/?\z",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

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
        var handle = ProductHandle(store, url);
        var sourceUrl = $"https://{store.Host}/products/{handle}";
        var productPath = $"/products/{handle}.js";

        // El portal de robots se consulta antes que la ficha: una ruta excluida no llega a pedirse.
        var policy = await _robots.GetAsync(store.Host, cancellationToken);
        if (!policy.Allows(productPath))
            throw new StorePathDisallowedException(store.StoreName, store.Host, productPath);

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);
        var token = timeout.Token;
        using var response = await _limiter.SendAsync(store.Host, attempt => SendAsync(sourceUrl + ".js", attempt), token);
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new StoreRateLimitException(store.StoreName, RetryGuidance(response));
        // El transporte registrado desactiva las redirecciones. También se rechazan en los transportes personalizados.
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaximumBytes)
            throw new InvalidDataException("La respuesta supera el límite permitido.");

        var body = await ReadAsync(response, token);
        try
        {
            using var document = JsonDocument.Parse(body);
            var product = document.RootElement;
            var variants = product.GetProperty("variants").EnumerateArray().Select(variant =>
            {
                var price = Price(variant.GetProperty("price"));
                decimal? original = variant.TryGetProperty("compare_at_price", out var compare) &&
                    compare.ValueKind != JsonValueKind.Null ? Price(compare) : null;
                decimal? discount = original > price && original > 0
                    ? Math.Round((original.Value - price) / original.Value * 100m, 2) : null;
                return new IrionVariantPreviewDto(
                    variant.GetProperty("id").GetInt64().ToString(CultureInfo.InvariantCulture),
                    OptionalString(variant, "sku"), OptionalString(variant, "barcode"),
                    variant.GetProperty("available").GetBoolean(), price, original, discount);
            }).ToArray();
            if (variants.Length == 0)
                throw new InvalidDataException("El producto no contiene variantes.");

            // La moneda es la de la tienda, no una constante: una segunda fila puede cobrar en otra moneda.
            // El tipo de producto ("type" en Shopify) se lee aquí porque la ingesta clasifica con él antes que con
            // las etiquetas o el título; el campo vacío o ausente queda como "no informado".
            return new StoreProductSheet(sourceUrl,
                RequiredString(product, "title"), RequiredString(product, "vendor"), store.Currency,
                RequiredString(product, "description"),
                OptionalString(product, "type"),
                product.GetProperty("tags").EnumerateArray().Select(value => value.GetString() ??
                    throw new InvalidDataException("Etiqueta inválida.")).ToArray(),
                product.GetProperty("images").EnumerateArray().Select(value => ImageUrl(value.GetString())).ToArray(),
                product.GetProperty("available").GetBoolean(), variants);
        }
        catch (Exception exception) when (exception is KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new InvalidDataException("La respuesta no contiene un producto válido.", exception);
        }
    }

    private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.UserAgent.ParseAdd(UserAgent);
        return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private static async Task<byte[]> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
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

        return buffer.ToArray();
    }

    // Se valida sobre la ruta original: Uri normaliza los segmentos con punto y las barras invertidas, así que
    // la comprobación se hace sobre el texto recibido para no aceptar rutas normalizadas fuera del catálogo.
    // El descubrimiento reutiliza esta misma regla para no encolar rutas que la lectura rechazaría después.
    public static bool TryProductHandle(StoreContext store, string url, out string handle)
    {
        handle = string.Empty;
        if (string.IsNullOrWhiteSpace(url) || url.Length > MaximumUrlLength ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(StoreContext.NormalizeHost(uri.Host), store.Host, StringComparison.Ordinal) ||
            !uri.IsDefaultPort || uri.UserInfo.Length != 0)
            return false;

        var match = ProductPath.Match(RawPath(url));
        if (!match.Success) return false;
        handle = match.Groups[1].Value;
        return true;
    }

    private static string ProductHandle(StoreContext store, string url) =>
        TryProductHandle(store, url, out var handle) ? handle : throw StoreOfferUrlException.ForStore(store);

    // Desde el primer '/' después del esquema hasta '?' o '#': la ruta que realmente se escribió.
    private static string RawPath(string url)
    {
        var scheme = url.IndexOf("://", StringComparison.Ordinal);
        var start = scheme < 0 ? -1 : url.IndexOf('/', scheme + 3);
        if (start < 0) return string.Empty;
        var end = url.IndexOfAny(['?', '#'], start);
        return end < 0 ? url[start..] : url[start..end];
    }

    private static string? RetryGuidance(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var values)) return null;
        var headers = values.ToArray();
        if (headers.Length != 1 || !System.Net.Http.Headers.RetryConditionHeaderValue.TryParse(
                headers[0], out var retryAfter)) return null;

        if (retryAfter.Delta is { } delta && delta >= TimeSpan.Zero &&
            delta.TotalSeconds <= int.MaxValue)
            return $"Espera al menos {delta.TotalSeconds.ToString("0", CultureInfo.InvariantCulture)} segundos antes de reintentar manualmente.";
        if (retryAfter.Date is { } date && date > DateTimeOffset.UtcNow)
            return "No vuelvas a consultar antes de " +
                date.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
                " UTC; reintenta manualmente.";
        return null;
    }

    private static decimal Price(JsonElement value)
    {
        if (!value.TryGetInt64(out var amount) || amount < 0)
            throw new InvalidDataException("Precio inválido.");
        return amount / 100m;
    }

    private static string RequiredString(JsonElement value, string name) =>
        value.GetProperty(name).GetString() ?? throw new InvalidDataException("Campo de producto inválido.");

    private static string? OptionalString(JsonElement value, string name) =>
        value.TryGetProperty(name, out var field) && field.ValueKind != JsonValueKind.Null
            ? field.GetString() : null;

    private static string ImageUrl(string? value)
    {
        if (value?.StartsWith("//", StringComparison.Ordinal) == true) value = "https:" + value;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.UserInfo.Length != 0)
            throw new InvalidDataException("URL de imagen inválida.");
        return uri.AbsoluteUri;
    }
}
