using System.Net;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Infrastructure.Services;

// Descubrimiento por sitemap: se sigue sólo el índice y los sitemaps de producto, se obedece robots.txt, se
// respeta el presupuesto de peticiones y se registra lo descartado. Sin red y sin base de datos: el transporte es
// un doble que cuenta y responde lo que cada escenario necesita.
internal static class CatalogDiscoveryChecks
{
    private const string Host = "irion.cl";
    private const string ProductSitemap1 = "https://irion.cl/sitemap_products_1.xml";
    private const string ProductSitemap2 = "https://irion.cl/sitemap_products_2.xml";
    private const string CollectionsSitemap = "https://irion.cl/sitemap_collections_1.xml";
    private const string PagesSitemap = "https://irion.cl/sitemap_pages_1.xml";
    private const string First = "https://irion.cl/products/gundam-a";
    private const string Second = "https://irion.cl/products/gundam-b";
    private const string Third = "https://irion.cl/products/gundam-c";
    private const string Fourth = "https://irion.cl/products/gundam-d";
    private const string Fifth = "https://irion.cl/products/gundam-e";
    private const string Hidden = "https://irion.cl/products/oculto";
    private const string PermissiveRobots = "User-agent: *\nDisallow: /cart\nDisallow: /admin\n";

    public static async Task Run(Action<bool, string> check)
    {
        await FollowsOnlyProductSitemaps(check);
        await ReadsPlainUrlset(check);
        await StopsAtBudget(check);
        await ObeysRobots(check);
        await RecordsUnreadableSitemaps(check);
        await NeverLeavesTheStore(check);
        await DeduplicatesAndStaysDeterministic(check);
        await RefusesAnUnknownPlatform(check);
        await HonoursCancellation(check);
    }

    private static async Task FollowsOnlyProductSitemaps(Action<bool, string> check)
    {
        var scenario = Scenario(index: Index(CollectionsSitemap, ProductSitemap1, PagesSitemap, ProductSitemap2),
            documents: new Dictionary<string, string>
            {
                [ProductSitemap1] = UrlSet(First, Second),
                [ProductSitemap2] = UrlSet(Third)
            }, latency: TimeSpan.FromMilliseconds(15));
        var result = await scenario.DiscoverAsync(new RequestBudget(50));

        check(result.ProductUrls.SequenceEqual(new[] { First, Second, Third }),
            "Product URLs are collected in sitemap order: " + string.Join(", ", result.ProductUrls));
        check(scenario.Transport.Uris.SequenceEqual(new[]
            {
                "https://irion.cl/robots.txt", "https://irion.cl/sitemap.xml", ProductSitemap1, ProductSitemap2
            }),
            "Only the index and the product sitemaps are requested: " + string.Join(", ", scenario.Transport.Uris));
        check(scenario.Transport.MaxConcurrency == 1,
            "The shared limiter serialises discovery requests against the same domain");
        check(result.CatalogSkipped.Select(skip => (skip.Reference, skip.Reason)).SequenceEqual(new[]
            {
                (CollectionsSitemap, CatalogSkipReason.NotProductSitemap),
                (PagesSitemap, CatalogSkipReason.NotProductSitemap)
            }),
            "Every sitemap that is not a product sitemap is recorded with its reason");
        check(result.DiscoveredCount == 3 && result.ProductSkipped.Count == 0 &&
            result.BudgetConsumed == 6,
            "The walk reports what it queued and what it spent: three URLs plus two documents plus the index");
    }

    private static async Task ReadsPlainUrlset(Action<bool, string> check)
    {
        var scenario = Scenario(index: UrlSet(First, Second));
        var result = await scenario.DiscoverAsync(new RequestBudget(10));
        check(result.ProductUrls.SequenceEqual(new[] { First, Second }) &&
            scenario.Transport.Uris.SequenceEqual(new[] { "https://irion.cl/robots.txt", "https://irion.cl/sitemap.xml" }),
            "A sitemap that is already a URL set is the catalogue, with no second request");
        check(result.CatalogSkipped.Count == 0 && result.BudgetConsumed == 3,
            "A plain URL set spends one request for the document and one per queued URL");
    }

    private static async Task StopsAtBudget(Action<bool, string> check)
    {
        // (a) El recorrido se detiene exactamente en el presupuesto y nunca lo supera.
        foreach (var total in new[] { 1, 2, 3, 4, 5, 6, 9 })
        {
            var scenario = Scenario(index: Index(ProductSitemap1),
                documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First, Second, Third, Fourth, Fifth) });
            var budget = new RequestBudget(total);
            var result = await scenario.DiscoverAsync(budget);
            var requests = scenario.Transport.Uris.Count(uri => !uri.EndsWith("/robots.txt", StringComparison.Ordinal));
            check(result.ProductUrls.Count <= total && result.BudgetConsumed == budget.Consumed &&
                budget.Consumed <= total && requests <= total,
                $"Discovery never exceeds the budget of {total}: {result.ProductUrls.Count} URLs, " +
                $"{requests} requests, {budget.Consumed} consumed");
            check(result.ProductUrls.Count == Math.Min(Math.Max(total - 2, 0), 5),
                $"Discovery queues exactly what the budget of {total} allows: {result.ProductUrls.Count}");
            if (total >= 3 && total < 7)
                check(result.CatalogSkipped.Any(skip => skip.Reason == CatalogSkipReason.BudgetExhausted),
                    $"Running out of budget at {total} is recorded instead of being silent");
            if (total >= 7)
                check(result.CatalogSkipped.All(skip => skip.Reason != CatalogSkipReason.BudgetExhausted),
                    $"With enough budget the whole catalogue is read at {total}");
        }

        var tiny = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First) });
        var empty = await tiny.DiscoverAsync(new RequestBudget(2));
        check(empty.ProductUrls.Count == 0 &&
            tiny.Transport.Uris.SequenceEqual(new[] { "https://irion.cl/robots.txt", "https://irion.cl/sitemap.xml" }) &&
            empty.CatalogSkipped.SingleOrDefault() is { Reason: CatalogSkipReason.BudgetExhausted },
            "A budget that cannot pay for a document plus one product is recorded, not overspent");

        // La URL encolada reserva su propia petición: el presupuesto vale para la ejecución completa.
        var documentOnly = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First, Second) });
        var reserved = await documentOnly.DiscoverAsync(new RequestBudget(4));
        check(reserved.ProductUrls.Count == 2 && reserved.BudgetConsumed == 4,
            "Each queued product reserves the request that reading it will cost");
    }

    private static async Task ObeysRobots(Action<bool, string> check)
    {
        var deniedSitemap = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First) },
            robots: "User-agent: *\nDisallow: /sitemap.xml\n");
        var denied = await deniedSitemap.DiscoverAsync(new RequestBudget(10));
        check(denied.ProductUrls.Count == 0 &&
            deniedSitemap.Transport.Uris.SequenceEqual(new[] { "https://irion.cl/robots.txt" }) &&
            denied.CatalogSkipped.SingleOrDefault() is
                { Reference: "https://irion.cl/sitemap.xml", Reason: CatalogSkipReason.RobotsDisallowed },
            "A disallowed sitemap is skipped and recorded without any HTTP request");

        // (b) Una ruta de producto prohibida se descarta y se registra: no se pide y no se encola.
        var deniedProduct = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First, Hidden, Second) },
            robots: PermissiveRobots + "Disallow: /products/oculto\n");
        var partial = await deniedProduct.DiscoverAsync(new RequestBudget(20));
        check(partial.ProductUrls.SequenceEqual(new[] { First, Second }),
            "The allowed product URLs are queued and the disallowed one is not: " +
            string.Join(", ", partial.ProductUrls));
        check(partial.ProductSkipped.SingleOrDefault() is
                { Reference: Hidden, Reason: CatalogSkipReason.RobotsDisallowed } excluded &&
            excluded.Message == "robots.txt prohíbe esta ruta: no se consultó.",
            "The disallowed product path is recorded with its reason");
        check(partial.ProductUrls.All(url => !url.Contains("oculto", StringComparison.Ordinal)) &&
            !deniedProduct.Transport.Uris.Any(uri => uri.Contains("oculto", StringComparison.Ordinal)),
            "A disallowed product path is never requested");
        check(partial.DiscoveredCount == 2 && partial.BudgetConsumed == 4,
            "A product rejected by the shared rule is not queued, not counted and does not reserve a request");

        var unreadable = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First) },
            robots: null, robotsStatus: HttpStatusCode.Forbidden);
        var refused = await unreadable.DiscoverAsync(new RequestBudget(10));
        check(refused.ProductUrls.Count == 0 &&
            unreadable.Transport.Uris.SequenceEqual(new[] { "https://irion.cl/robots.txt" }) &&
            refused.CatalogSkipped.SingleOrDefault() is { Reason: CatalogSkipReason.RobotsDisallowed },
            "An unreadable robots.txt is a prohibition, not permission");
    }

    private static async Task RecordsUnreadableSitemaps(Action<bool, string> check)
    {
        var scenario = Scenario(index: Index(ProductSitemap1, ProductSitemap2),
            documents: new Dictionary<string, string> { [ProductSitemap2] = UrlSet(Third) },
            missing: [ProductSitemap1]);
        var result = await scenario.DiscoverAsync(new RequestBudget(20));
        check(result.CatalogSkipped.Any(skip =>
                skip.Reference == ProductSitemap1 && skip.Reason == CatalogSkipReason.SitemapUnavailable),
            "A product sitemap that cannot be read is recorded and the walk continues");
        check(result.ProductUrls.SequenceEqual(new[] { Third }),
            "The next readable product sitemap still contributes its URLs");

        var malformed = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = "<urlset><loc>sin cerrar" });
        var broken = await malformed.DiscoverAsync(new RequestBudget(20));
        check(broken.ProductUrls.Count == 0 &&
            broken.CatalogSkipped.SingleOrDefault() is
                { Reference: ProductSitemap1, Reason: CatalogSkipReason.SitemapUnavailable },
            "A malformed sitemap is recorded as unreadable instead of guessing URLs");

        var refused = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First) },
            robots: PermissiveRobots + $"Disallow: {ProductSitemap1[("https://" + Host).Length..]}\n");
        var noDocument = await refused.DiscoverAsync(new RequestBudget(20));
        check(noDocument.ProductUrls.Count == 0 &&
            noDocument.CatalogSkipped.Any(skip =>
                skip.Reference == ProductSitemap1 && skip.Reason == CatalogSkipReason.RobotsDisallowed) &&
            !refused.Transport.Uris.Contains(ProductSitemap1),
            "A product sitemap forbidden by robots.txt is recorded without being requested");
    }

    private static async Task NeverLeavesTheStore(Action<bool, string> check)
    {
        var scenario = Scenario(index: Index("https://evil.test/sitemap_products_1.xml", ProductSitemap1),
            documents: new Dictionary<string, string>
            {
                [ProductSitemap1] = UrlSet(First, "https://evil.test/products/other", "https://irion.cl/collections/all")
            });
        var result = await scenario.DiscoverAsync(new RequestBudget(20));
        check(result.ProductUrls.SequenceEqual(new[] { First }),
            "Only product pages of the configured store are queued");
        check(!scenario.Transport.Uris.Any(uri => uri.Contains("evil.test", StringComparison.Ordinal)),
            "A foreign host is never requested, not even when the sitemap lists it");
        check(result.CatalogSkipped.Any(skip =>
                skip.Reference == "https://evil.test/sitemap_products_1.xml" &&
                skip.Reason == CatalogSkipReason.NotProductSitemap) &&
            result.ProductSkipped.Count == 2 &&
            result.ProductSkipped.All(skip => skip.Reason == CatalogSkipReason.NotProductUrl),
            "A foreign sitemap and a non-product entry are recorded with their reasons");
        check(result.DiscoveredCount == result.ProductUrls.Count && result.DiscoveredCount == 1,
            "The discovered counter is the queued items, not every entry the sitemap listed");
    }

    private static async Task DeduplicatesAndStaysDeterministic(Action<bool, string> check)
    {
        var scenario = () => Scenario(index: Index(ProductSitemap1, ProductSitemap2),
            documents: new Dictionary<string, string>
            {
                [ProductSitemap1] = UrlSet(First, Second), [ProductSitemap2] = UrlSet(Second, Third)
            });
        var first = await scenario().DiscoverAsync(new RequestBudget(20));
        var second = await scenario().DiscoverAsync(new RequestBudget(20));
        check(first.ProductUrls.SequenceEqual(new[] { First, Second, Third }),
            "A URL repeated across product sitemaps is queued only once");
        check(first.ProductUrls.SequenceEqual(second.ProductUrls),
            "Two walks of the same sitemap produce the same order");
        check(first.DiscoveredCount == 3 && first.DiscoveredCount == first.ProductUrls.Count,
            "Discovery counts each queued URL once, including the deduplication of an exact repeat");
    }

    // La regla de "es una ficha de esta tienda" no es sólo la forma de la ruta: incluye la plataforma que declara la
    // fila. Una tienda cuyo conector no conoce esa forma no puede encolar rutas que el conector va a rechazar.
    private static async Task RefusesAnUnknownPlatform(Action<bool, string> check)
    {
        var scenario = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First, Second) });
        var other = new StoreContext(Guid.NewGuid(), "Koneko", Host, "html-selectors", "CLP", 20, 500);

        var result = await scenario.DiscoverAsync(other, new RequestBudget(20));

        check(result.ProductUrls.Count == 0 && result.DiscoveredCount == 0 &&
            result.ProductSkipped.Select(skip => (skip.Reference, skip.Reason)).SequenceEqual(new[]
            {
                (First, CatalogSkipReason.NotProductUrl), (Second, CatalogSkipReason.NotProductUrl)
            }),
            "A store whose connector the rule does not know yet queues nothing and records every entry");
        check(result.ProductSkipped.All(skip => skip.Message == CatalogSkip.NotProductUrlMessage),
            "The skip keeps the reason the connector would have given for the same entry");
    }

    private static async Task HonoursCancellation(Action<bool, string> check)
    {
        var scenario = Scenario(index: Index(ProductSitemap1),
            documents: new Dictionary<string, string> { [ProductSitemap1] = UrlSet(First) });
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        var rejected = false;
        try
        {
            await scenario.Discovery.DiscoverAsync(Store, new RequestBudget(10), cancelled.Token);
        }
        catch (OperationCanceledException)
        {
            rejected = true;
        }

        check(rejected && scenario.Transport.Uris.Count == 0,
            "A cancelled discovery does not reach the network");
    }

    private static StoreContext Store { get; } =
        new(Guid.NewGuid(), "Irion", Host, ShopifyConnector.ConnectorKey, "CLP", 20, 500);

    private static Walk Scenario(string index, Dictionary<string, string>? documents = null,
        string? robots = PermissiveRobots, HttpStatusCode robotsStatus = HttpStatusCode.OK,
        string[]? missing = null, TimeSpan latency = default)
    {
        var transport = new FakeTransport { Latency = latency };
        var catalog = documents ?? [];
        transport.Respond = request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path == "/robots.txt")
                return robotsStatus == HttpStatusCode.OK
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(robots ?? "") }
                    : new HttpResponseMessage(robotsStatus);
            var uri = request.RequestUri.AbsoluteUri;
            if (uri == "https://irion.cl/sitemap.xml")
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(index) };
            if (missing is not null && missing.Contains(uri))
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            return catalog.TryGetValue(uri, out var body)
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }
                : new HttpResponseMessage(HttpStatusCode.NotFound);
        };

        var http = new HttpClient(transport);
        var limiter = new DomainRequestLimiter((_, _) => Task.CompletedTask);
        return new Walk(transport, new SitemapCatalogDiscovery(http, new RobotsTxtCache(http, limiter), limiter));
    }

    private static string Index(params string[] locations) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
        {string.Join("\n", locations.Select(location => $"  <sitemap><loc>{location}</loc></sitemap>"))}
        </sitemapindex>
        """;

    private static string UrlSet(params string[] locations) => $"""
        <?xml version="1.0" encoding="UTF-8"?>
        <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
        {string.Join("\n", locations.Select(location => $"  <url><loc>{location}</loc></url>"))}
        </urlset>
        """;

    private sealed record Walk(FakeTransport Transport, SitemapCatalogDiscovery Discovery)
    {
        public Task<CatalogDiscoveryResult> DiscoverAsync(RequestBudget budget) =>
            DiscoverAsync(CatalogDiscoveryChecks.Store, budget);

        public Task<CatalogDiscoveryResult> DiscoverAsync(StoreContext store, RequestBudget budget) =>
            Discovery.DiscoverAsync(store, budget, default);
    }
}
