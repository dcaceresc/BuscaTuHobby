using System.Net;
using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Infrastructure.Services;

// Conector Shopify generalizado: host y moneda vienen de la fila de la tienda, robots.txt se obedece y el
// limitador impone una petición a la vez con retroceso acotado. Sin base de datos y sin red: el transporte es
// un doble que cuenta y responde lo que cada escenario necesita.
const string irionUrl = "https://irion.cl/products/optimus-prime";
const string luffyUrl = "https://luffytoys.cl/products/gundam-rx-78";
const string fixture = Fixtures.Body;
const string permissiveRobots = "User-agent: *\nDisallow: /cart\nDisallow: /admin\n";
const string urlMessage =
    "Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto";
const string rateLimitMessage = "Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.";

var irion = new StoreContext(Guid.NewGuid(), "Irion", "irion.cl", ShopifyConnector.ConnectorKey, "CLP", 20, 500);
var luffy = new StoreContext(Guid.NewGuid(), "LuffyToys", "luffytoys.cl", ShopifyConnector.ConnectorKey, "USD", 10, 100);

var count = 0;
var failures = new List<string>();
void Check(bool condition, string message)
{
    if (!condition) failures.Add(message);
    count++;
}

// (a) Segunda tienda Shopify configurada por fila: el mismo conector produce una oferta normalizada, con su
// propio host y su propia moneda, sin tocar código.
var second = new Scenario(permissiveRobots);
var luffyPreview = await second.Connector.LoadProductAsync(luffy, luffyUrl, default);
Check(luffyPreview.SourceUrl == luffyUrl && luffyPreview.Currency == "USD",
    "A second store row yields its own canonical source and currency through the same connector");
Check(luffyPreview.Title == "Optimus Prime" && luffyPreview.Variants[0].Price == 21990m &&
    luffyPreview.Variants[1].DiscountPercentage == 20.01m,
    "The second store produces the same normalised offer shape");
Check(second.Transport.Uris.SequenceEqual(new[] { "https://luffytoys.cl/robots.txt", luffyUrl + ".js" }),
    "The second store fetches only its own host, robots first: " + string.Join(", ", second.Transport.Uris));
var irionPreview = await second.Connector.LoadProductAsync(irion, irionUrl, default);
Check(irionPreview.Currency == "CLP" && irionPreview.SourceUrl == irionUrl &&
    second.Transport.Uris.Count == 4,
    "Irion keeps working as the first configured row, not as a special class");

// El cargador elige el conector por la clave de la fila y resuelve la tienda por el host de la URL.
var provider = new RecordingProvider(irion, luffy);
var loader = new StoreProductLoader(provider, [second.Connector]);
var loaded = await loader.LoadAsync("https://www." + luffyUrl["https://".Length..], default);
Check(loaded.Currency == "USD" && provider.Hosts.SequenceEqual(new[] { "luffytoys.cl" }) &&
    second.Transport.Uris.Count == 5,
    "Adding a row is enough: the loader resolves the store by host and reuses the registered connector");
await RejectAsync(() => loader.LoadAsync("https://unknown.test/products/x", default), urlMessage);
Check(second.Transport.Uris.Count == 5, "An unknown host never reaches the transport");

// Contrato de producto: URL canónica, campos, precios y variantes (comportamiento anterior conservado).
var product = new Scenario(permissiveRobots);
var preview = await product.Connector.LoadProductAsync(irion, irionUrl + "?_su_rec=tracking#details", default);
Check(product.Transport.LastUri == irionUrl + ".js", "Must build canonical URL without tracking");
Check(preview.SourceUrl == irionUrl && preview.Currency == "CLP", "Canonical source and store currency");
Check(preview.Title == "Optimus Prime" && preview.Vendor == "Blokees" && preview.Available, "Product identity");
Check(preview.DescriptionHtml == "<p>13cm, 8 años +</p>" && preview.Tags.Single() == "TRANSFORMERS", "Description and tags");
Check(preview.ImageUrls.Single() == "https://cdn.shopify.com/figure.png", "Normalize image URLs without fetching");
Check(preview.Variants.Count == 2 && preview.Variants[0].Price == 21990m, "Prices must be converted from hundredths");
Check(preview.Variants[0].OriginalPrice is null && preview.Variants[0].DiscountPercentage is null, "Missing reference price is not a discount");
Check(preview.Variants[0].Sku == "BLOK-71428" && preview.Variants[0].Barcode == "810181537222", "Identifiers");
Check(preview.Variants[1].Sku is null && !preview.Variants[1].Available && preview.Variants[1].OriginalPrice == 24990m, "Variant data is independent");
Check(preview.Variants[1].DiscountPercentage == 20.01m, "Discount calculation");

// La ingesta clasifica con el tipo de producto que publica la tienda; el preview manual sigue entregando el mismo
// DTO, así que la ficha tipada y la vista previa tienen que describir el mismo producto.
var sheet = await product.Connector.LoadSheetAsync(irion, irionUrl, default);
Check(sheet.ProductType == "Model Kit" && sheet.DeclaredProductType == "Model Kit",
    "The store product type is read and exposed for ingestion");
Check(sheet.ToPreview() is var projected && projected.Title == preview.Title &&
    projected.Vendor == preview.Vendor && projected.Currency == preview.Currency &&
    projected.DescriptionHtml == preview.DescriptionHtml && projected.Available == preview.Available &&
    projected.Tags.SequenceEqual(preview.Tags) && projected.ImageUrls.SequenceEqual(preview.ImageUrls) &&
    projected.Variants[0] == preview.Variants[0] && projected.Variants[1] == preview.Variants[1],
    "The manual preview projection keeps every published field unchanged");
var untyped = new Scenario(permissiveRobots, _ => new HttpResponseMessage(HttpStatusCode.OK)
    { Content = new StringContent(fixture.Replace("\"type\":\"Model Kit\",", string.Empty)) });
Check((await untyped.Connector.LoadSheetAsync(irion, irionUrl, default)).DeclaredProductType is null,
    "A sheet that does not declare a product type leaves it as not informed");

// Una URL que no es de producto no llega ni a robots.txt.
foreach (var url in new[] { "", "http://irion.cl/products/a", "https://evil.test/products/a",
    "https://irion.cl.evil.test/products/a", "https://user@irion.cl/products/a",
    "https://irion.cl:444/products/a", "https://irion.cl/admin", "https://irion.cl/products/a.js",
    "https://irion.cl/products/a/b", "https://irion.cl/products/../admin",
    "https://irion.cl/products/%2e%2e", "https://irion.cl/products/a%2fb",
    "https://irion.cl/products/a\\b", "https://irion.cl/collections/all/products/a/b",
    "https://irion.cl/collections/../products/a", "https://irion.cl/collections/all/products/%2e%2e",
    "https://irion.cl/collections/all/products/a%2fb", "https://irion.cl/collections/a%2fb/products/a",
    "https://irion.cl/collections/all/products/a%5cb", "https://irion.cl/collections/all/products/a\\b",
    "https://irion.cl/collections/all/../products/a", "https://irion.cl/collections/all/products/a.js",
    "https://irion.cl/collections/-all/products/a", "https://irion.cl/collections/all/products/a--b",
    "https://irion.cl/collections/álbum/products/a", "https://evil.test/collections/all/products/a",
    "https://user@irion.cl/collections/all/products/a", "https://irion.cl:444/collections/all/products/a" })
{
    var scenario = new Scenario(permissiveRobots);
    await RejectAsync(() => scenario.Connector.LoadProductAsync(irion, url, default),
        urlMessage);
    Check(scenario.Transport.Uris.Count == 0, "Invalid URL must not cause any network request: " + url);
}

// (e) Una sola regla para "esto es una ficha de esta tienda": el conector acepta exactamente lo que la regla
// compartida acepta y rechaza el resto sin pedir nada, así que el descubrimiento no puede encolar lo que la lectura
// rechazaría después.
foreach (var (candidate, accepted) in new[]
{
    (irionUrl, true),
    ("https://irion.cl/collections/productos-mas-recientes/products/optimus-prime", true),
    ("https://irion.cl/collections/all", false),
    ("https://irion.cl/products/optimus-prime/extra", false),
    ("https://luffytoys.cl/products/optimus-prime", false)
})
{
    var shared = new Scenario(permissiveRobots);
    var acceptedByRule = ProductSheetUrl.TryGetHandle(irion, candidate, out var handle);
    Check(acceptedByRule == accepted && (!accepted || handle == "optimus-prime"),
        "The shared product-sheet rule answers from the store and the URL: " + candidate);
    if (accepted)
        Check((await shared.Connector.LoadProductAsync(irion, candidate, default)).Title == "Optimus Prime" &&
            shared.Transport.ProductCalls == 1,
            "The connector loads the URL the shared rule accepts: " + candidate);
    else
        await RejectAsync(() => shared.Connector.LoadProductAsync(irion, candidate, default), urlMessage);
    Check(shared.Transport.ProductCalls == (accepted ? 1 : 0),
        "The connector never fetches a URL the shared rule rejects: " + candidate);
}

// La plataforma de la fila forma parte de la regla: una tienda cuyo conector no declara forma de ficha no acepta
// ninguna URL, en vez de colar la forma de otra plataforma.
var otherPlatform = new StoreContext(Guid.NewGuid(), "Pasatime", "irion.cl", "json-ld", "CLP", 20, 50);
Check(!ProductSheetUrl.TryGetHandle(otherPlatform, irionUrl, out _),
    "A store whose platform declares no sheet shape accepts no URL");

// Una sola petición a la vez por dominio, también cuando dos fichas de la misma tienda se piden juntas.
var serialized = new Scenario(permissiveRobots, latency: TimeSpan.FromMilliseconds(15));
await Task.WhenAll(
    serialized.Connector.LoadProductAsync(irion, irionUrl, default),
    serialized.Connector.LoadProductAsync(irion, "https://irion.cl/products/other-kit", default));
Check(serialized.Transport.MaxConcurrency == 1 && serialized.Transport.ProductCalls == 2,
    "Requests to the same domain are serialized, never parallel");

// (b) El portal de robots se obedece y se cachea por host: una ruta excluida no se pide nunca.
var disallowed = new Scenario("User-agent: *\nDisallow: /products/optimus-prime.js\n");
await RejectAsync(() => disallowed.Connector.LoadProductAsync(irion, irionUrl, default),
    "robots.txt de irion.cl excluye /products/optimus-prime.js: no se consultó la ficha de Irion.");
Check(disallowed.Transport.Uris.SequenceEqual(new[] { "https://irion.cl/robots.txt" }),
    "A disallowed path is refused without requesting the product");
var deniedPrefix = new Scenario("User-agent: *\nDisallow: /products/\n");
await RejectAsync(() => deniedPrefix.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(deniedPrefix.Transport.ProductCalls == 0, "A disallowed prefix never reaches the product endpoint");
var allowedOverride = new Scenario("User-agent: *\nDisallow: /products/\nAllow: /products/optimus-prime.js\n");
Check((await allowedOverride.Connector.LoadProductAsync(irion, irionUrl, default)).Title == "Optimus Prime" &&
    allowedOverride.Transport.ProductCalls == 1,
    "The most specific allow wins over a broader disallow");
var namedAgent = new Scenario(
    "User-agent: BuscaTuHobby\nDisallow: /products/\n\nUser-agent: *\nDisallow:\n");
await RejectAsync(() => namedAgent.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(namedAgent.Transport.ProductCalls == 0, "The group naming our agent wins over the wildcard group");
var wildcard = new Scenario("User-agent: *\nDisallow: /*.js$\n");
await RejectAsync(() => wildcard.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(wildcard.Transport.ProductCalls == 0, "robots wildcards and end anchors are honoured");
var missingRobots = new Scenario(null, null, HttpStatusCode.NotFound);
Check((await missingRobots.Connector.LoadProductAsync(irion, irionUrl, default)).Title == "Optimus Prime" &&
    missingRobots.Transport.ProductCalls == 1,
    "An absent robots.txt allows the product (nothing was forbidden)");
var forbiddenRobots = new Scenario(null, null, HttpStatusCode.Forbidden);
await RejectAsync(() => forbiddenRobots.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(forbiddenRobots.Transport.ProductCalls == 0,
    "An unreadable robots.txt (403) is treated as a prohibition, not as permission");
var flakyRobots = new Scenario(null, null, HttpStatusCode.TooManyRequests);
Check((await flakyRobots.Connector.LoadProductAsync(irion, irionUrl, default)).Title == "Optimus Prime",
    "A transient robots.txt failure does not turn the import into a permanent error");
var cached = new Scenario(permissiveRobots);
await cached.Connector.LoadProductAsync(irion, irionUrl, default);
await cached.Connector.LoadProductAsync(irion, luffyUrl.Replace("luffytoys.cl", "irion.cl").Replace("gundam-rx-78", "other-kit"), default);
Check(cached.Transport.RobotsCalls == 1 && cached.Transport.ProductCalls == 2,
    "robots.txt is fetched once per host and reused");

// (c) 429: se espera lo que pide Retry-After y se reintenta de forma acotada.
var retried = new Scenario(permissiveRobots, RetryThenOk(HttpStatusCode.TooManyRequests, "7"));
var retriedPreview = await retried.Connector.LoadProductAsync(irion, irionUrl, default);
Check(retriedPreview.Title == "Optimus Prime" && retried.Transport.ProductCalls == 2 &&
    retried.Delays.SequenceEqual(new[] { TimeSpan.FromSeconds(7) }),
    "A 429 retries once after exactly the Retry-After delay");
var exhausted = new Scenario(permissiveRobots, Always(HttpStatusCode.TooManyRequests, "7"));
await RejectAsync(() => exhausted.Connector.LoadProductAsync(irion, irionUrl, default),
    "Irion está limitando las consultas (HTTP 429). Espera al menos 7 segundos antes de reintentar manualmente.");
Check(exhausted.Transport.ProductCalls == 3 &&
    exhausted.Delays.SequenceEqual(new[] { TimeSpan.FromSeconds(7), TimeSpan.FromSeconds(7) }) &&
    exhausted.Transport.ProductCalls == exhausted.Transport.Uris.Count - 1,
    "Retries are bounded and never fall back to another endpoint");
var transient = new Scenario(permissiveRobots, RetryThenOk(HttpStatusCode.InternalServerError, null));
Check((await transient.Connector.LoadProductAsync(irion, irionUrl, default)).Title == "Optimus Prime" &&
    transient.Delays.SequenceEqual(new[] { TimeSpan.FromSeconds(1) }),
    "5xx retries with exponential backoff when no Retry-After is sent");
var alwaysBroken = new Scenario(permissiveRobots, Always(HttpStatusCode.BadGateway, null));
await RejectAsync(() => alwaysBroken.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(alwaysBroken.Transport.ProductCalls == 3 &&
    alwaysBroken.Delays.SequenceEqual(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }),
    "Server errors stop after the bounded number of attempts");
var invalidRetryAfter = new Scenario(permissiveRobots, Always(HttpStatusCode.TooManyRequests, "-5"));
await RejectAsync(() => invalidRetryAfter.Connector.LoadProductAsync(irion, irionUrl, default),
    rateLimitMessage);
Check(invalidRetryAfter.Delays.SequenceEqual(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }),
    "An invalid Retry-After falls back to the exponential backoff");

// (d) Ningún 4xx que no sea 429 se reintenta.
foreach (var status in new[] { HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized,
    HttpStatusCode.Forbidden, HttpStatusCode.NotFound, HttpStatusCode.Gone })
{
    var clientError = new Scenario(permissiveRobots, Always(status, "5"));
    await RejectAsync(() => clientError.Connector.LoadProductAsync(irion, irionUrl, default), null);
    Check(clientError.Transport.ProductCalls == 1 && clientError.Delays.Count == 0,
        "A " + (int)status + " is not retried");
}

foreach (var status in new[] { HttpStatusCode.MovedPermanently, HttpStatusCode.Found, HttpStatusCode.SeeOther,
    HttpStatusCode.TemporaryRedirect, HttpStatusCode.PermanentRedirect })
{
    var redirect = new Scenario(permissiveRobots, Always(status, null));
    await RejectAsync(() => redirect.Connector.LoadProductAsync(irion, irionUrl, default), null);
    Check(redirect.Transport.ProductCalls == 1, "Redirections are never followed: " + (int)status);
}

foreach (var body in new[] { "not json", "{}", "[]", fixture.Replace("2199000", "-100"),
    fixture.Replace("\"available\":true", "\"available\":null"), fixture.Replace("\"variants\":[", "\"variants\":[] ,\"unused\":[") })
{
    var malformed = new Scenario(permissiveRobots, _ => new HttpResponseMessage(HttpStatusCode.OK)
        { Content = new StringContent(body) });
    await RejectAsync(() => malformed.Connector.LoadProductAsync(irion, irionUrl, default), null);
}

var oversized = new Scenario(permissiveRobots, _ => new HttpResponseMessage(HttpStatusCode.OK)
    { Content = new StringContent(new string('x', 1_048_577)) });
await RejectAsync(() => oversized.Connector.LoadProductAsync(irion, irionUrl, default), null);
Check(oversized.Transport.ProductCalls == 1, "The one MiB cap is still enforced after a single request");

var cancelledScenario = new Scenario(permissiveRobots);
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
await RejectAsync(() => cancelledScenario.Connector.LoadProductAsync(irion, irionUrl, cancelled.Token), null);
Check(cancelledScenario.Transport.Uris.Count == 0, "A cancelled call does not reach the network");

// Descubrimiento por sitemap: orden determinista, presupuesto respetado y descartes registrados.
await CatalogDiscoveryChecks.Run(Check);

// La ruta existente del mantenedor conserva sus mensajes exactos y su envoltura.
var handlerScenario = new Scenario(permissiveRobots);
var handler = new PreviewIrionProductHandler(
    new StoreProductLoader(new RecordingProvider(irion, luffy), [handlerScenario.Connector]),
    new ApiResponseService());
var success = await handler.Handle(new PreviewIrionProduct(irionUrl), default);
Check(success.Success && success.Message is null && success.Data.Variants[0].Price == 21990m,
    "Public handler returns the detached preview through the configured store");
foreach (var (input, canonical) in new[] {
    ("https://irion.cl/products/bandai-v-gundam-ver-ka-mg-1100?_pos=1&_sid=1e1ac7564&_ss=r",
        "https://irion.cl/products/bandai-v-gundam-ver-ka-mg-1100"),
    ("https://irion.cl/collections/productos-mas-recientes/products/transformers-action-edition-07-optimus-prime-ver-2-0",
        "https://irion.cl/products/transformers-action-edition-07-optimus-prime-ver-2-0") })
{
    var calls = handlerScenario.Transport.ProductCalls;
    var result = await handler.Handle(new PreviewIrionProduct(input), default);
    Check(result.Success && result.Data?.SourceUrl == canonical && result.Data.Title == "Optimus Prime",
        "Supplied URL must produce successful canonical preview: " + input);
    Check(handlerScenario.Transport.ProductCalls == calls + 1 &&
        handlerScenario.Transport.LastUri == canonical + ".js",
        "Supplied URL must make exactly one canonical request: " + input);
}

var handlers = new (IStoreProductLoader Loader, string Message)[]
{
    (new ThrowingLoader(new StoreOfferUrlException()), urlMessage),
    (new ThrowingLoader(new StoreRateLimitException("Irion")), rateLimitMessage),
    (new ThrowingLoader(new StorePathDisallowedException("Irion", "irion.cl", "/products/x.js")),
        "robots.txt de irion.cl excluye /products/x.js: no se consultó la ficha de Irion."),
    (new ThrowingLoader(new InvalidDataException("private")),
        "No se pudo obtener la vista previa del producto de Irion.")
};
foreach (var (failureLoader, message) in handlers)
{
    var failure = await new PreviewIrionProductHandler(failureLoader, new ApiResponseService())
        .Handle(new PreviewIrionProduct(irionUrl), default);
    Check(!failure.Success && failure.Data is null && failure.Message == message,
        "The preview envelope keeps the exact Spanish message: " + message);
}

foreach (var url in new[] { "https://evil.test/products/a", "https://irion.cl/admin" })
{
    var calls = handlerScenario.Transport.Uris.Count;
    var failure = await handler.Handle(new PreviewIrionProduct(url), default);
    Check(!failure.Success && failure.Data is null && failure.Message == urlMessage,
        "Invalid URL must explain both accepted formats");
    Check(handlerScenario.Transport.Uris.Count == calls, "Invalid URL handler must not make a request");
}

const string rateLimitPrefix = "Irion está limitando las consultas (HTTP 429). ";
foreach (var (retryAfter, expectedMessage) in new (string?, string)[] {
    (null, rateLimitMessage), ("invalid", rateLimitMessage), ("-5", rateLimitMessage),
    ("30", rateLimitPrefix + "Espera al menos 30 segundos antes de reintentar manualmente."),
    ("0", rateLimitPrefix + "Espera al menos 0 segundos antes de reintentar manualmente."),
    ("2147483647", rateLimitPrefix + "Espera al menos 2147483647 segundos antes de reintentar manualmente."),
    ("2147483648", rateLimitMessage),
    ("Thu, 01 Jan 2099 00:00:00 GMT", rateLimitPrefix + "No vuelvas a consultar antes de 2099-01-01 00:00:00 UTC; reintenta manualmente."),
    ("Sat, 01 Jan 2000 00:00:00 GMT", rateLimitMessage),
    ("999999999999999999999999999", rateLimitMessage) })
{
    var limited = new Scenario(permissiveRobots, Always(HttpStatusCode.TooManyRequests, retryAfter));
    var result = await new PreviewIrionProductHandler(
            new StoreProductLoader(new RecordingProvider(irion, luffy), [limited.Connector]),
            new ApiResponseService())
        .Handle(new PreviewIrionProduct(irionUrl), default);
    Check(!result.Success && result.Data is null && result.Message == expectedMessage,
        "429 must return specific Spanish envelope (Retry-After: " + (retryAfter ?? "absent") + ")");
    Check(limited.Transport.ProductCalls == 3 && limited.Delays.Count == 2,
        "429 keeps the bounded retry policy and never uses a fallback endpoint");
}

if (failures.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
    Console.Error.WriteLine($"FAIL: {failures.Count} of {count} assertions.");
    return 1;
}

Console.WriteLine($"PASS: {count} assertions; no database or live HTTP used.");
return 0;

static Func<HttpRequestMessage, HttpResponseMessage> Always(HttpStatusCode status, string? retryAfter) =>
    _ =>
    {
        var response = new HttpResponseMessage(status);
        if (retryAfter is not null) response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        return response;
    };

static Func<HttpRequestMessage, HttpResponseMessage> RetryThenOk(HttpStatusCode status, string? retryAfter)
{
    var attempts = 0;
    return request =>
    {
        if (attempts++ == 0)
        {
            var response = new HttpResponseMessage(status);
            if (retryAfter is not null) response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            return response;
        }

        return Fixtures.Product();
    };
}


async Task RejectAsync(Func<Task<IrionProductPreviewDto>> action, string? message)
{
    try
    {
        await action();
    }
    catch (Exception exception) when (exception is StoreOfferUrlException or StoreRateLimitException
        or StorePathDisallowedException or StoreConfigurationException or HttpRequestException
        or InvalidDataException or System.Text.Json.JsonException or OperationCanceledException)
    {
        if (message is not null && exception.Message != message)
            throw new Exception($"Unexpected message: {exception.Message}");
        count++;
        return;
    }

    throw new Exception("Expected rejection: " + (message ?? "any behaviour-level failure"));
}

sealed class Scenario
{
    public Scenario(string? robots, Func<HttpRequestMessage, HttpResponseMessage>? product = null,
        HttpStatusCode robotsStatus = HttpStatusCode.OK, TimeSpan latency = default)
    {
        Transport.Latency = latency;
        Transport.Respond = request =>
            request.RequestUri!.AbsolutePath == "/robots.txt"
                ? robotsStatus == HttpStatusCode.OK
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(robots ?? "") }
                    : new HttpResponseMessage(robotsStatus)
                : (product ?? (_ => Fixtures.Product()))(request);
        var http = new HttpClient(Transport);
        var limiter = new DomainRequestLimiter((delay, _) =>
        {
            Delays.Add(delay);
            return Task.CompletedTask;
        });
        Connector = new ShopifyConnector(http, new RobotsTxtCache(http, limiter), limiter);
    }

    public FakeTransport Transport { get; } = new();
    public List<TimeSpan> Delays { get; } = [];
    public ShopifyConnector Connector { get; }
}

sealed class Fixtures
{
    // Producto Shopify mínimo con lo que el conector normaliza: título, etiquetas, imágenes y variantes.
    public const string Body = """
{"title":"Optimus Prime","vendor":"Blokees","tags":["TRANSFORMERS"],"type":"Model Kit",
"description":"<p>13cm, 8 años +</p>","images":["//cdn.shopify.com/figure.png"],
"available":true,"variants":[
{"id":123,"sku":"BLOK-71428","barcode":"810181537222","available":true,"price":2199000,"compare_at_price":null},
{"id":124,"sku":null,"barcode":null,"available":false,"price":1999000,"compare_at_price":2499000}]}
""";

    public static HttpResponseMessage Product() =>
        new(HttpStatusCode.OK) { Content = new StringContent(Body) };
}

sealed class FakeTransport : HttpMessageHandler
{
    private int _inFlight;

    public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } = _ => new(HttpStatusCode.OK);
    public List<string> Uris { get; } = [];
    public TimeSpan Latency { get; set; }
    public int MaxConcurrency { get; private set; }

    public string? LastUri => Uris.Count == 0 ? null : Uris[^1];
    public int RobotsCalls => Uris.Count(uri => uri.EndsWith("/robots.txt", StringComparison.Ordinal));
    public int ProductCalls => Uris.Count(uri => uri.EndsWith(".js", StringComparison.Ordinal));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Uris.Add(request.RequestUri!.AbsoluteUri);
        MaxConcurrency = Math.Max(MaxConcurrency, ++_inFlight);
        try
        {
            if (Latency > TimeSpan.Zero) await Task.Delay(Latency, cancellationToken);
            return Respond(request);
        }
        finally
        {
            _inFlight--;
        }
    }
}

sealed class RecordingProvider(params StoreContext[] stores) : IStoreContextProvider
{
    public List<string> Hosts { get; } = [];

    public Task<StoreContext?> FindByHostAsync(string host, CancellationToken cancellationToken)
    {
        Hosts.Add(host);
        return Task.FromResult(stores.SingleOrDefault(store => store.Host == host));
    }
}

sealed class ThrowingLoader(Exception error) : IStoreProductLoader
{
    public Task<IrionProductPreviewDto> LoadAsync(string url, CancellationToken cancellationToken) =>
        Task.FromException<IrionProductPreviewDto>(error);
}
