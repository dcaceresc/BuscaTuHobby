using System.Net.Http.Headers;
using System.Collections.Concurrent;

namespace Infrastructure.Services;

/// <summary>
/// Cortesía por dominio, no por llamador: una petición a la vez contra un mismo host, <c>Retry-After</c>
/// respetado, retroceso exponencial en 429/5xx y ningún reintento en el resto de 4xx. El número de intentos
/// está acotado para que ninguna ficha pueda quedar en un bucle.
/// </summary>
public sealed class DomainRequestLimiter
{
    public const int MaximumAttempts = 3;

    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaximumDelay = TimeSpan.FromSeconds(30);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gates = new(StringComparer.OrdinalIgnoreCase);
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public DomainRequestLimiter() : this(static (delay, token) => Task.Delay(delay, token))
    {
    }

    // El reloj se inyecta para que las pruebas del retroceso sean deterministas y sin esperas reales.
    public DomainRequestLimiter(Func<TimeSpan, CancellationToken, Task> delay)
    {
        _delay = delay;
    }

    /// <summary>
    /// Ejecuta un intento y lo repite solo ante un fallo transitorio. Devuelve la última respuesta, con o sin
    /// éxito, para que el conector decida el mensaje; los 4xx que no son 429 se devuelven de inmediato.
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(
        string host, Func<CancellationToken, Task<HttpResponseMessage>> attempt, CancellationToken cancellationToken)
    {
        var gate = _gates.GetOrAdd(host, static _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            for (var number = 1; ; number++)
            {
                var response = await attempt(cancellationToken);
                if (number >= MaximumAttempts || !Transient(response.StatusCode))
                    return response;

                // Se calcula antes de liberar la respuesta porque Retry-After vive en sus cabeceras.
                var wait = RetryAfter(response) ?? Backoff(number);
                response.Dispose();
                await _delay(wait, cancellationToken);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    private static bool Transient(System.Net.HttpStatusCode status) =>
        status == System.Net.HttpStatusCode.TooManyRequests || (int)status >= 500;

    // Con Retry-After se espera exactamente lo que pide el servidor: acortarlo sería desobedecerlo.
    private static TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Retry-After", out var values)) return null;
        var headers = values.ToArray();
        if (headers.Length != 1 || !RetryConditionHeaderValue.TryParse(headers[0], out var parsed)) return null;
        if (parsed.Delta is { } delta) return delta >= TimeSpan.Zero ? delta : null;
        if (parsed.Date is { } date && date > DateTimeOffset.UtcNow) return date - DateTimeOffset.UtcNow;
        return null;
    }

    // Sin Retry-After el retroceso crece 1 s, 2 s, 4 s… con tope; nunca se desborda ni espera para siempre.
    private static TimeSpan Backoff(int attempt)
    {
        var seconds = BaseDelay.TotalSeconds * Math.Pow(2, attempt - 1);
        return seconds >= MaximumDelay.TotalSeconds ? MaximumDelay : TimeSpan.FromSeconds(seconds);
    }
}
