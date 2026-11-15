using System.Globalization;
using IrionRateLimitException = Application.Common.Exceptions.IrionRateLimitException;
using IrionProductUrlException = Application.Common.Exceptions.IrionProductUrlException;
using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Infrastructure.Services;

public class IrionProductExtractor(HttpClient client) : IIrionProductExtractor
{
    private readonly HttpClient _client = client;
    private const int MaximumBytes = 1_048_576;
    private static readonly Regex ProductPath = new(
        @"\A/(?:collections/[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*/)?products/([a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*)/?\z",
        RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public async Task<IrionProductPreviewDto> ExtractAsync(string url, CancellationToken cancellationToken)
    {
        // Check the original path as well: Uri normalizes dot segments and backslashes.
        if (string.IsNullOrWhiteSpace(url) || url.Length > 8192 ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.Host != "irion.cl" ||
            !uri.IsDefaultPort || uri.UserInfo.Length != 0)
            throw new IrionProductUrlException();

        var rawPath = Regex.Match(url, @"\Ahttps://irion\.cl(?::443)?(/[^?#]*)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));
        var match = ProductPath.Match(rawPath.Success ? rawPath.Groups[1].Value : "");
        if (!match.Success)
            throw new IrionProductUrlException();

        var sourceUrl = "https://irion.cl/products/" + match.Groups[1].Value;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        var token = timeout.Token;
        using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl + ".js");
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.UserAgent.ParseAdd("BuscaTuHobby-ProductPreview/1.0");
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        // The registered transport disables redirects. Also reject redirects in custom transports.
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            throw new IrionRateLimitException(RetryGuidance(response));
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > MaximumBytes)
            throw new InvalidDataException("La respuesta supera el límite permitido.");

        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(chunk, token)) > 0)
        {
            if (buffer.Length + read > MaximumBytes)
                throw new InvalidDataException("La respuesta supera el límite permitido.");
            await buffer.WriteAsync(chunk.AsMemory(0, read), token);
        }

        try
        {
            using var document = JsonDocument.Parse(buffer.ToArray());
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

            return new IrionProductPreviewDto(sourceUrl,
                RequiredString(product, "title"), RequiredString(product, "vendor"), "CLP",
                RequiredString(product, "description"),
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
