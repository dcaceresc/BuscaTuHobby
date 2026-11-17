using System.Globalization;
using System.Net.Http.Headers;

namespace Infrastructure.Services;

/// <summary>
/// Texto de espera que se ofrece a una persona cuando la tienda responde 429. Vive en un solo lugar para que todas
/// las plataformas expliquen el mismo <c>Retry-After</c> con la misma frase y ningún conector invente una espera.
/// </summary>
internal static class StoreRetryGuidance
{
    public static string? From(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var values)) return null;
        var headers = values.ToArray();
        if (headers.Length != 1 || !RetryConditionHeaderValue.TryParse(headers[0], out var retryAfter)) return null;

        if (retryAfter.Delta is { } delta && delta >= TimeSpan.Zero && delta.TotalSeconds <= int.MaxValue)
            return $"Espera al menos {delta.TotalSeconds.ToString("0", CultureInfo.InvariantCulture)} segundos antes de reintentar manualmente.";
        if (retryAfter.Date is { } date && date > DateTimeOffset.UtcNow)
            return "No vuelvas a consultar antes de " +
                date.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
                " UTC; reintenta manualmente.";
        return null;
    }
}
