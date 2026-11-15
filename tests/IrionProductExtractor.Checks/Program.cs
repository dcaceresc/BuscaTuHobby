using System.Net;
using System.Text;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;
using Infrastructure.Services;

const string source = "https://irion.cl/products/optimus-prime";
const string fixture = """
{"title":"Optimus Prime","vendor":"Blokees","tags":["TRANSFORMERS"],
"description":"<p>13cm, 8 años +</p>","images":["//cdn.shopify.com/figure.png"],
"available":true,"variants":[
{"id":123,"sku":"BLOK-71428","barcode":"810181537222","available":true,"price":2199000,"compare_at_price":null},
{"id":124,"sku":null,"barcode":null,"available":false,"price":1999000,"compare_at_price":2499000}]}
""";
var count = 0;
var failures = new List<string>();
void Check(bool condition, string message)
{
    if (!condition) failures.Add(message);
    count++;
}
var transport = new FakeTransport();
using var client = new HttpClient(transport);
var extractor = new IrionProductExtractor(client);
transport.Respond = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(fixture) };
var preview = await extractor.ExtractAsync(source + "?_su_rec=tracking#details", default);
Check(transport.LastUri == source + ".js", "Must build canonical URL without tracking");
Check(preview.SourceUrl == source && preview.Currency == "CLP", "Canonical source and currency");
Check(preview.Title == "Optimus Prime" && preview.Vendor == "Blokees" && preview.Available, "Product identity");
Check(preview.DescriptionHtml == "<p>13cm, 8 años +</p>" && preview.Tags.Single() == "TRANSFORMERS", "Description and tags");
Check(preview.ImageUrls.Single() == "https://cdn.shopify.com/figure.png", "Normalize image URLs without fetching");
Check(preview.Variants.Count == 2 && preview.Variants[0].Price == 21990m, "Prices must be converted from hundredths");
Check(preview.Variants[0].OriginalPrice is null && preview.Variants[0].DiscountPercentage is null, "Missing reference price is not a discount");
Check(preview.Variants[0].Sku == "BLOK-71428" && preview.Variants[0].Barcode == "810181537222", "Identifiers");
Check(preview.Variants[1].Sku is null && !preview.Variants[1].Available && preview.Variants[1].OriginalPrice == 24990m, "Variant data is independent");
Check(preview.Variants[1].DiscountPercentage == 20.01m, "Discount calculation");

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
    var calls = transport.Calls;
    await Reject(() => extractor.ExtractAsync(url, default));
    Check(transport.Calls == calls, "Invalid URL must not cause any network request: " + url);
}
foreach (var status in new[] { 301, 302, 303, 307, 308, 404, 500 })
{
    transport.Respond = () => new HttpResponseMessage((HttpStatusCode)status) { Headers = { Location = new Uri("https://evil.test/") } };
    await Reject(() => extractor.ExtractAsync(source, default));
}
foreach (var body in new[] { "not json", "{}", "[]", fixture.Replace("2199000", "-100"),
    fixture.Replace("\"available\":true", "\"available\":null"), fixture.Replace("\"variants\":[", "\"variants\":[] ,\"unused\":[") })
{
    transport.Respond = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
    await Reject(() => extractor.ExtractAsync(source, default));
}
transport.Respond = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(new string('x', 1_048_577)) };
await Reject(() => extractor.ExtractAsync(source, default));
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
await Reject(() => extractor.ExtractAsync(source, cancelled.Token));
var handler = new PreviewIrionProductHandler(extractor, new ApiResponseService());
const string urlMessage = "Introduce una URL válida de Irion: https://irion.cl/products/nombre-del-producto o https://irion.cl/collections/coleccion/products/nombre-del-producto";
foreach (var invalid in new[] { "https://evil.test/products/a", "https://irion.cl/admin" })
{
    var calls = transport.Calls;
    var failure = await handler.Handle(new PreviewIrionProduct(invalid), default);
    Check(!failure.Success && failure.Data is null && failure.Message == urlMessage,
        "Invalid URL must explain both accepted formats");
    Check(transport.Calls == calls, "Invalid URL handler must not make a request");
}
transport.Respond = () => new HttpResponseMessage(HttpStatusCode.InternalServerError);
var genericFailure = await handler.Handle(new PreviewIrionProduct(source), default);
Check(!genericFailure.Success && genericFailure.Data is null &&
    genericFailure.Message == "No se pudo obtener la vista previa del producto de Irion.", "Other failures remain generic");
transport.Respond = () => throw new ArgumentException("Private transport detail");
var argumentFailure = await handler.Handle(new PreviewIrionProduct(source), default);
Check(!argumentFailure.Success && argumentFailure.Data is null &&
    argumentFailure.Message == genericFailure.Message, "Unrelated argument errors must not leak details");
transport.Respond = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(fixture) };
var success = await handler.Handle(new PreviewIrionProduct(source), default);
Check(success.Success && success.Data.Variants[0].Price == 21990m, "Public handler returns detached preview");
foreach (var (input, canonical) in new[] {
    ("https://irion.cl/products/bandai-v-gundam-ver-ka-mg-1100?_pos=1&_sid=1e1ac7564&_ss=r",
        "https://irion.cl/products/bandai-v-gundam-ver-ka-mg-1100"),
    ("https://irion.cl/collections/productos-mas-recientes/products/transformers-action-edition-07-optimus-prime-ver-2-0",
        "https://irion.cl/products/transformers-action-edition-07-optimus-prime-ver-2-0") })
{
    var calls = transport.Calls;
    var result = await handler.Handle(new PreviewIrionProduct(input), default);
    Check(result.Success && result.Data?.SourceUrl == canonical && result.Data.Title == "Optimus Prime",
        "Supplied URL must produce successful canonical preview: " + input);
    Check(transport.Calls == calls + 1 && transport.LastUri == canonical + ".js",
        "Supplied URL must make exactly one canonical request: " + input);
}
const string rateLimitMessage = "Irion está limitando las consultas (HTTP 429). Inténtalo más tarde.";
foreach (var (retryAfter, expectedMessage) in new (string?, string)[] {
    (null, rateLimitMessage), ("invalid", rateLimitMessage), ("-5", rateLimitMessage),
    ("30", "Irion está limitando las consultas (HTTP 429). Espera al menos 30 segundos antes de reintentar manualmente."),
    ("0", "Irion está limitando las consultas (HTTP 429). Espera al menos 0 segundos antes de reintentar manualmente."),
    ("2147483647", "Irion está limitando las consultas (HTTP 429). Espera al menos 2147483647 segundos antes de reintentar manualmente."),
    ("2147483648", rateLimitMessage),
    ("Thu, 01 Jan 2099 00:00:00 GMT", "Irion está limitando las consultas (HTTP 429). No vuelvas a consultar antes de 2099-01-01 00:00:00 UTC; reintenta manualmente."),
    ("Sat, 01 Jan 2000 00:00:00 GMT", rateLimitMessage),
    ("999999999999999999999999999", rateLimitMessage) })
{
    transport.Respond = () => {
        var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        if (retryAfter is not null) response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
        return response;
    };
    var calls = transport.Calls;
    var result = await handler.Handle(new PreviewIrionProduct(source), default);
    Check(!result.Success && result.Data is null && result.Message == expectedMessage,
        "429 must return specific Spanish envelope (Retry-After: " + (retryAfter ?? "absent") + ")");
    Check(transport.Calls == calls + 1, "429 must not retry or use fallback endpoints");
}
if (failures.Count > 0)
    throw new Exception(string.Join(Environment.NewLine, failures));
Console.WriteLine($"PASS: {count} assertions; no database or live HTTP used.");

async Task Reject(Func<Task<IrionProductPreviewDto>> action)
{
    try { await action(); }
    catch (Exception ex) when (ex is Application.Common.Exceptions.IrionProductUrlException or ArgumentException or HttpRequestException or System.Text.Json.JsonException or InvalidDataException or OperationCanceledException)
    { count++; return; }
    throw new Exception("Expected rejection");
}

sealed class FakeTransport : HttpMessageHandler
{
    public Func<HttpResponseMessage> Respond { get; set; } = null!;
    public string? LastUri { get; private set; }
    public int Calls { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls++;
        LastUri = request.RequestUri!.AbsoluteUri;
        return Task.FromResult(Respond());
    }
}
