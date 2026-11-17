using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

/// <summary>
/// Reglas de <c>robots.txt</c> que aplican a nuestro agente. El grupo que nombra al agente manda sobre el
/// grupo comodín, y dentro del grupo gana la regla más específica; a igual longitud, permite.
/// </summary>
public sealed class RobotsTxtPolicy
{
    private readonly (string Source, Regex Pattern, bool Allow)[] _rules;
    private readonly bool _denyAll;

    private RobotsTxtPolicy((string Source, Regex Pattern, bool Allow)[] rules, bool denyAll = false)
    {
        _rules = rules;
        _denyAll = denyAll;
    }

    public static RobotsTxtPolicy AllowAll { get; } = new([]);

    // 401/403 sobre robots.txt significa que el archivo existe pero no se puede leer: no se asume permiso.
    public static RobotsTxtPolicy DenyAll { get; } = new([], denyAll: true);

    public bool Allows(string path)
    {
        if (_denyAll) return false;
        var allowed = true;
        var best = -1;
        foreach (var (source, pattern, allow) in _rules)
        {
            if (!pattern.IsMatch(path)) continue;
            if (source.Length > best)
            {
                best = source.Length;
                allowed = allow;
            }
            else if (source.Length == best && allow)
            {
                allowed = true;
            }
        }

        return allowed;
    }

    public static RobotsTxtPolicy Parse(string content, string userAgent)
    {
        var groups = new List<(List<string> Agents, List<(string Pattern, bool Allow)> Rules)>();
        var agents = new List<string>();
        var rules = new List<(string Pattern, bool Allow)>();
        var lastWasAgent = false;
        foreach (var raw in content.Split('\n'))
        {
            var line = raw;
            var comment = line.IndexOf('#');
            if (comment >= 0) line = line[..comment];
            line = line.Trim();
            if (line.Length == 0) continue;
            var colon = line.IndexOf(':');
            if (colon <= 0) continue;
            var field = line[..colon].Trim().ToLowerInvariant();
            var value = line[(colon + 1)..].Trim();
            if (field == "user-agent")
            {
                // Varias líneas user-agent consecutivas forman un grupo; una nueva después de reglas, otro.
                if (!lastWasAgent && agents.Count > 0)
                {
                    groups.Add((agents, rules));
                    agents = [];
                    rules = [];
                }

                agents.Add(value);
                lastWasAgent = true;
                continue;
            }

            lastWasAgent = false;
            // Un Disallow vacío no restringe nada: no se convierte en una regla que excluya todo.
            if (field is "allow" or "disallow" && value.Length > 0)
                rules.Add((value, field == "allow"));
        }

        if (agents.Count > 0) groups.Add((agents, rules));

        var named = groups
            .SelectMany(group => group.Agents
                .Where(agent => agent != "*" && userAgent.Contains(agent, StringComparison.OrdinalIgnoreCase))
                .Select(agent => (agent.Length, group.Rules)))
            .OrderByDescending(candidate => candidate.Length)
            .FirstOrDefault();
        var wildcard = groups.FirstOrDefault(group => group.Agents.Contains("*"));

        var selected = named.Rules ?? wildcard.Rules ?? [];
        return new RobotsTxtPolicy(selected.Select(rule => (rule.Pattern, Compile(rule.Pattern), rule.Allow)).ToArray());
    }

    private static Regex Compile(string pattern)
    {
        // Soporte mínimo del estándar: '*' comodín y '$' como ancla de fin, que es lo que usan las tiendas.
        var anchored = pattern.EndsWith('$');
        var body = anchored ? pattern[..^1] : pattern;
        var escaped = Regex.Escape(body).Replace("\\*", ".*", StringComparison.Ordinal);
        return new Regex("\\A" + escaped + (anchored ? "\\z" : string.Empty),
            RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    }
}

/// <summary>
/// Descarga y cachea el <c>robots.txt</c> de cada host una sola vez. Sin caché, cada ficha pagaría una
/// petición extra por el mismo archivo.
/// </summary>
public sealed class RobotsTxtCache(HttpClient client, DomainRequestLimiter limiter)
{
    private const int MaximumBytes = 262_144;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
    private readonly HttpClient _client = client;
    private readonly ConcurrentDictionary<string, Lazy<Task<RobotsTxtPolicy>>> _hosts =
        new(StringComparer.OrdinalIgnoreCase);

    public async Task<RobotsTxtPolicy> GetAsync(string host, CancellationToken cancellationToken)
    {
        // Se comprueba antes de crear la entrada: una llamada ya cancelada no debe salir a la red.
        cancellationToken.ThrowIfCancellationRequested();
        var entry = _hosts.GetOrAdd(host, key => new Lazy<Task<RobotsTxtPolicy>>(() => FetchAsync(key)));
        return await entry.Value.WaitAsync(cancellationToken);
    }

    private async Task<RobotsTxtPolicy> FetchAsync(string host)
    {
        using var timeout = new CancellationTokenSource(Timeout);
        try
        {
            using var response = await limiter.SendAsync(host, token => SendAsync(host, token), timeout.Token);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return RobotsTxtPolicy.DenyAll;
            if (!response.IsSuccessStatusCode)
                return RobotsTxtPolicy.AllowAll;

            var content = await ReadAsync(response, timeout.Token);
            return content is null ? RobotsTxtPolicy.AllowAll : RobotsTxtPolicy.Parse(content, ShopifyConnector.UserAgent);
        }
        catch (Exception)
        {
            // Un fallo de transporte al leer robots.txt no detiene la importación: se permite y el limitador
            // sigue imponiendo una petición a la vez, que es la protección que no depende de la tienda.
            return RobotsTxtPolicy.AllowAll;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(string host, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://{host}/robots.txt");
        request.Headers.UserAgent.ParseAdd(ShopifyConnector.UserAgent);
        return await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private static async Task<string?> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength > MaximumBytes) return null;
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;
        while ((read = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + read > MaximumBytes) return null;
            await buffer.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }
}
