using System.Text;
using System.Xml;
using System.Xml.Linq;
using Application.Common.Ingestion;

namespace Infrastructure.Services;

// Recorrido del catálogo por el sitemap que publica la tienda (familia Shopify: índice sitemap.xml → sitemaps de
// producto sitemap_products_* → URL de ficha). No adivina rutas, no sale del host configurado y cada petición pasa
// por el mismo limitador y el mismo portal de robots que la lectura de una ficha.
public sealed class SitemapCatalogDiscovery(HttpClient client, RobotsTxtCache robots, DomainRequestLimiter limiter)
    : ICatalogDiscovery
{
    public const string SitemapPath = "/sitemap.xml";
    public const string ProductSitemapMarker = "sitemap_products_";

    // Un sitemap de producto de Shopify ronda el millón de bytes; el tope evita descargar un archivo desmedido.
    private const int MaximumBytes = 4_194_304;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

    private readonly HttpClient _client = client;
    private readonly RobotsTxtCache _robots = robots;
    private readonly DomainRequestLimiter _limiter = limiter;

    public async Task<CatalogDiscoveryResult> DiscoverAsync(
        StoreContext store, RequestBudget budget, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(budget);

        var policy = await _robots.GetAsync(store.Host, cancellationToken);
        var indexUrl = $"https://{store.Host}{SitemapPath}";
        var catalogSkipped = new List<CatalogSkip>();
        var productSkipped = new List<CatalogSkip>();
        var urls = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var documents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var discovered = 0;
        var exhausted = false;

        // Encola una entrada del listado: deduplica, descarta lo que no es una ficha y respeta el presupuesto.
        // Cada URL encolada reserva la petición que costará leerla, así el tope vale para la ejecución completa.
        void Enqueue(string documentUrl, IReadOnlyList<string> locations)
        {
            foreach (var candidate in locations)
            {
                if (exhausted) return;
                discovered++;
                if (!seen.Add(candidate)) continue;
                if (!ShopifyConnector.TryProductHandle(store, candidate, out _))
                {
                    productSkipped.Add(new CatalogSkip(candidate, CatalogSkipReason.NotProductUrl));
                    continue;
                }

                if (!policy.Allows(PathOf(candidate)))
                {
                    productSkipped.Add(new CatalogSkip(candidate, CatalogSkipReason.RobotsDisallowed));
                    continue;
                }

                if (!budget.TryConsume())
                {
                    catalogSkipped.Add(new CatalogSkip(documentUrl, CatalogSkipReason.BudgetExhausted));
                    exhausted = true;
                    return;
                }

                urls.Add(candidate);
            }
        }

        if (!policy.Allows(SitemapPath))
        {
            catalogSkipped.Add(new CatalogSkip(indexUrl, CatalogSkipReason.RobotsDisallowed));
            return Result();
        }

        if (!budget.TryConsume())
        {
            catalogSkipped.Add(new CatalogSkip(indexUrl, CatalogSkipReason.BudgetExhausted));
            return Result();
        }

        var index = await TryReadAsync(store.Host, indexUrl, catalogSkipped, cancellationToken);
        if (index is not null)
        {
            if (!IsIndex(index))
            {
                // sitemap.xml puede ser ya el listado de fichas: sus entradas son el catálogo.
                Enqueue(indexUrl, Locations(index));
            }
            else
            {
                foreach (var location in Locations(index))
                {
                    if (exhausted) break;
                    if (!documents.Add(location)) continue;
                    if (!IsProductSitemap(store, location))
                    {
                        catalogSkipped.Add(new CatalogSkip(location, CatalogSkipReason.NotProductSitemap));
                        continue;
                    }

                    // El portal de robots se consulta también para el documento: una ruta excluida no se descarga.
                    if (!policy.Allows(PathOf(location)))
                    {
                        catalogSkipped.Add(new CatalogSkip(location, CatalogSkipReason.RobotsDisallowed));
                        continue;
                    }

                    // Un documento cuesta una petición y hace falta al menos otra para encolar una ficha: gastar la
                    // última en descargar un archivo que no se podrá leer no respeta la intención del tope.
                    if (budget.Remaining < 2)
                    {
                        catalogSkipped.Add(new CatalogSkip(location, CatalogSkipReason.BudgetExhausted));
                        exhausted = true;
                        break;
                    }

                    budget.TryConsume();
                    var document = await TryReadAsync(store.Host, location, catalogSkipped, cancellationToken);
                    if (document is null) continue;
                    Enqueue(location, Locations(document));
                }
            }
        }

        return Result();

        CatalogDiscoveryResult Result() =>
            new(urls, catalogSkipped, productSkipped, discovered, budget.Consumed);
    }

    private async Task<XDocument?> TryReadAsync(
        string host, string url, List<CatalogSkip> skipped, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(RequestTimeout);
        try
        {
            using var response = await _limiter.SendAsync(host, attempt => SendAsync(url, attempt), timeout.Token);
            if (!response.IsSuccessStatusCode || response.Content.Headers.ContentLength > MaximumBytes)
            {
                skipped.Add(new CatalogSkip(url, CatalogSkipReason.SitemapUnavailable));
                return null;
            }

            return XDocument.Parse(await ReadAsync(response, timeout.Token));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
            or XmlException or InvalidDataException)
        {
            // La cancelación del llamador no es un sitemap ilegible: se propaga tal cual.
            cancellationToken.ThrowIfCancellationRequested();
            skipped.Add(new CatalogSkip(url, CatalogSkipReason.SitemapUnavailable));
            return null;
        }
    }

    private async Task<HttpResponseMessage> SendAsync(string url, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.ParseAdd("application/xml");
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

    private static bool IsIndex(XDocument document) => document.Root?.Name.LocalName == "sitemapindex";

    // Se leen por nombre local para no depender del prefijo de espacio de nombres que use la tienda.
    private static IReadOnlyList<string> Locations(XDocument document) => document.Descendants()
        .Where(element => element.Name.LocalName == "loc")
        .Select(element => element.Value.Trim())
        .Where(value => value.Length > 0)
        .ToList();

    // Sólo se siguen los sitemaps de producto del propio host: un índice puede listar otro dominio y el recorrido
    // nunca debe salir de la tienda configurada.
    private static bool IsProductSitemap(StoreContext store, string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps && uri.IsDefaultPort && uri.UserInfo.Length == 0 &&
        string.Equals(StoreContext.NormalizeHost(uri.Host), store.Host, StringComparison.Ordinal) &&
        uri.AbsolutePath.Contains(ProductSitemapMarker, StringComparison.Ordinal);

    // La URL ya se validó como ficha de producto, así que la ruta normalizada coincide con la que se escribió.
    private static string PathOf(string url) => new Uri(url).AbsolutePath;
}
