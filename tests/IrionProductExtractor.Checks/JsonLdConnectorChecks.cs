using System.Net;
using Application.Common.Exceptions;
using Application.Common.Ingestion;
using Application.Common.Interfaces;
using Infrastructure.Services;

// Conector JSON-LD y su forma de ficha: parser de bloques ld+json contra fichas con la forma real medida en las
// tiendas chilenas, negativa explicita cuando la pagina no publica un producto, descubrimiento por el sitemap de la
// propia tienda y el mismo portal de robots. Sin red y sin base de datos: el transporte es un doble que responde lo
// que cada escenario necesita.
internal static class JsonLdConnectorChecks
{
    private const string PermissiveRobots = "User-agent: *\nDisallow: /cart\nDisallow: /admin\n";
    private const string PasatimeUrl = "https://pasatime.cl/producto/tamiya-40mm-cinta-de-enmascarar-azul/";
    private const string SvaderUrl = "https://svadercoleccionables.cl/dv8-1/2-comic-en-ingles";
    private const string TitoysUrl = "https://titoys.cl/auto-de-juguete-colo-colo-164-licencia-oficial";
    private const string OtakuversoListing = "https://otakuverso.cl/productos";

    public static async Task Run(Action<bool, string> check)
    {
        await RealShapedSheets(check);
        await ListingPages(check);
        await Refusals(check);
        await Availability(check);
        await PriceAndIdentifierShapes(check);
        await SharedSheetShape(check);
        await Discovery(check);
        await RobotsGate(check);
        await Redirects(check);
    }

    // (a) Una ficha con la forma real de cada tienda produce la ficha esperada. Las tres publican lo mismo con
    // identificadores distintos: pasatime trae SKU, GTIN y marca, svader un precio decimal y nada mas, titoys un SKU
    // sin GTIN.
    private static async Task RealShapedSheets(Action<bool, string> check)
    {
        var pasatime = Scenario(Pasatime, PasatimePage);
        var sheet = await pasatime.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(sheet.Title == "Tamiya 40mm Cinta de Enmascarar (Azul) • TAM87240 • Pasatime" &&
            sheet.Vendor == "Tamiya" && sheet.Currency == "CLP" && sheet.Available,
            "Pasatime: title, brand as vendor, store currency and stock come from the published page");
        check(sheet.SourceUrl == PasatimeUrl &&
            sheet.Variants.Single() is { Sku: "TAM87240", Barcode: "4950344872404", Price: 6390m } variant &&
            variant.OriginalPrice is null && variant.DiscountPercentage is null,
            "Pasatime: sku, gtin13 and a localised price become one normalised offer");
        check(sheet.DeclaredProductType == "Modelismo" &&
            sheet.ImageUrls.SequenceEqual(new[] { "https://pasatime.cl/img/tamiya-87240.jpg" }) &&
            pasatime.Transport.Uris.SequenceEqual(new[]
            {
                "https://pasatime.cl/robots.txt", PasatimeUrl
            }),
            "Pasatime: category, image and the exact two requests (robots first, then the sheet)");
        check(sheet.ToPreview() is var preview && preview.SourceUrl == sheet.SourceUrl &&
            preview.Title == sheet.Title && preview.Vendor == sheet.Vendor &&
            preview.Currency == sheet.Currency && preview.Available == sheet.Available &&
            preview.Variants[0] == sheet.Variants[0],
            "The manual preview keeps the published DTO shape unchanged for this connector");

        var svader = Scenario(Svader, SvaderPage);
        var comic = await svader.Connector.LoadSheetAsync(Svader, SvaderUrl, default);
        check(comic.Title == "Dv8 #1/2" && comic.Vendor == string.Empty && comic.Currency == "CLP" &&
            comic.SourceUrl == SvaderUrl,
            "Svader: a sheet with no brand leaves the vendor empty instead of inventing one");
        check(comic.Variants.Single() is { Price: 1990m, Sku: null, Barcode: null } comicVariant &&
            comicVariant.Id == SvaderUrl && comicVariant.Available,
            "Svader: a decimal price and no identifiers still yield an offer identified by its sheet URL");

        var titoys = Scenario(Titoys, TitoysPage);
        var toy = await titoys.Connector.LoadSheetAsync(Titoys, TitoysUrl, default);
        check(toy.Title == "CAJA COMPLETA - Auto de Juguete Colo-Colo Escala 1:64" &&
            toy.Variants.Single() is { Sku: "TDC01", Barcode: null, Price: 108000m },
            "Titoys: the sku survives without a gtin and the price is not divided or rounded twice");
    }

    // (b) Un listado publica un nodo por tarjeta. No puede convertirse en una ficha que sume o mezcle productos.
    private static async Task ListingPages(Action<bool, string> check)
    {
        var listing = Scenario(Otakuverso, OtakuversoPage);
        await RejectAsync(() => listing.Connector.LoadSheetAsync(Otakuverso, OtakuversoListing, default),
            JsonLdConnector.AmbiguousProductMessage);
        check(listing.Transport.Uris.Count == 2,
            "A listing page is fetched once and refused, never expanded into several sheets");

        var other = Scenario(Otakuverso, OtakuversoPage);
        var chosen = await other.Connector.LoadSheetAsync(Otakuverso,
            "https://otakuverso.cl/productos/mazinger-z-infinity", default);
        check(chosen.Title == "Mazinger Z Infinity" && chosen.Variants.Single().Price == 6990m,
            "A listing that declares one node for the requested URL yields exactly that product");

        var anonymous = Scenario(Otakuverso, OtakuversoPage.Replace("\"url\"", "\"@id-unused\""));
        await RejectAsync(() => anonymous.Connector.LoadSheetAsync(Otakuverso,
            "https://otakuverso.cl/productos/mazinger-z-infinity", default),
            JsonLdConnector.AmbiguousProductMessage);
        check(anonymous.Transport.Uris.Count == 2,
            "Several undeclared products stay refused: guessing one would claim a sheet nobody declares");
    }

    // (c) Una pagina sin producto se niega con un motivo en espanol: no se convierte en una oferta vacia.
    private static async Task Refusals(Action<bool, string> check)
    {
        foreach (var (body, description) in new[]
        {
            (CategoryPage, "a category page with breadcrumbs but no product"),
            ("<html><body><h1>Sin datos estructurados</h1></body></html>", "a page with no ld+json block"),
            ("<script type=\"application/ld+json\">{ roto </script>", "a page whose only block is broken"),
            ("<script type=\"application/ld+json\">{&quot;@type&quot;:&quot;Product&quot;}</script>",
                "a page that publishes a non-product type")
        })
        {
            var scenario = Scenario(Pasatime, body);
            await RejectAsync(() => scenario.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
                JsonLdConnector.NoProductNodeMessage);
            check(scenario.Transport.Uris.Count == 2,
                "A refused page costs exactly one request: " + description);
        }

        var missingName = Scenario(Pasatime, PasatimePage.Replace(
            "\"name\":\"Tamiya 40mm Cinta de Enmascarar (Azul) • TAM87240 • Pasatime\",", string.Empty));
        await RejectAsync(() => missingName.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
            JsonLdConnector.MissingTitleMessage);
    }

    // (d) Sin disponibilidad declarada, una ficha con precio esta disponible; el ultimo segmento de la URL de
    // schema.org es lo que decide el estado.
    private static async Task Availability(Action<bool, string> check)
    {
        var noAvailability = await Scenario(Pasatime, WithAvailability(PasatimePage, null))
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(noAvailability.Available && noAvailability.Variants[0].Available,
            "A priced sheet without availability is available, not silently hidden");

        foreach (var (declared, expected) in new[]
        {
            ("https://schema.org/InStock", true),
            ("http://schema.org/InStock", true),
            ("InStock", true),
            ("https://schema.org/PreOrder", true),
            ("https://schema.org/BackOrder", true),
            ("https://schema.org/OutOfStock", false),
            ("https://schema.org/SoldOut", false),
            ("https://schema.org/Discontinued", false)
        })
        {
            var sheet = await Scenario(Pasatime, WithAvailability(PasatimePage, declared))
                .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
            check(sheet.Available == expected && sheet.Variants[0].Available == expected,
                "Availability is read from the last schema.org segment: " + declared);
        }
    }

    // Los precios medidos: entero, decimal de JSON y texto localizado. Un precio que no se puede leer se niega en vez
    // de publicarse como cero.
    private static async Task PriceAndIdentifierShapes(Action<bool, string> check)
    {
        foreach (var (price, expected) in new (string, decimal)[]
        {
            ("6390", 6390m),
            ("1990.0", 1990m),
            ("\"6390 CLP\"", 6390m),
            ("\"1990.0\"", 1990m),
            ("\"$ 6.390\"", 6390m),
            ("\"6.390,50\"", 6390.50m),
            ("\"6390.50\"", 6390.50m),
            ("\"1.990.000\"", 1990000m),
            ("\"6,50\"", 6.50m)
        })
        {
            var sheet = await Scenario(Pasatime, WithPrice(PasatimePage, price))
                .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
            check(sheet.Variants.Single().Price == expected,
                $"Price {price} is read as {expected}: {sheet.Variants.Single().Price}");
        }

        foreach (var price in new[] { "\"\"", "\"sin precio\"", "0", "-1" })
        {
            await RejectAsync(() => Scenario(Pasatime, WithPrice(PasatimePage, price))
                    .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
                JsonLdConnector.MissingPriceMessage);
        }

        var arrayOffers = """
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"Product","name":"Oferta en arreglo",
        "offers":[{"@type":"Offer","availability":"https://schema.org/InStock"},
        {"@type":"Offer","price":3490,"priceCurrency":"CLP","availability":"https://schema.org/InStock"}]}
        </script>
        """;
        var arraySheet = await Scenario(Pasatime, arrayOffers)
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(arraySheet.Variants.Single().Price == 3490m && arraySheet.Currency == "CLP",
            "An offers array is read in order and the first offer with a price is the one published");

        var group = """
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"ProductGroup","name":"Grupo con variantes",
        "brand":{"@type":"Brand","name":"Bandai"},"offers":{"@type":"Offer","price":12990,
        "availability":"https://schema.org/InStock"}}
        </script>
        """;
        var groupSheet = await Scenario(Pasatime, group)
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(groupSheet.Title == "Grupo con variantes" && groupSheet.Vendor == "Bandai" &&
            groupSheet.Variants.Single().Price == 12990m,
            "ProductGroup is accepted as a product node and keeps its brand and price");

        var graph = """
        <script type="application/ld+json">
        {"@context":"https://schema.org","@graph":[{"@type":"WebPage","name":"x"},
        {"@type":["Product","Thing"],"name":"Nodo en grafo","offers":{"price":"$1.990","priceCurrency":"CLP"}}]}
        </script>
        """;
        var graphSheet = await Scenario(Pasatime, graph)
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(graphSheet.Title == "Nodo en grafo" && graphSheet.Variants.Single().Price == 1990m,
            "A @graph with a Product node typed as an array is read as one sheet");

        var identifiers = """
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"Product","name":"Identificadores",
        "gtin":"00012345600012","gtin13":4950344872404,"ean":"4901234567894","sku":"TAM-1",
        "offers":{"price":1990,"priceCurrency":"CLP"}}
        </script>
        """;
        var identified = await Scenario(Pasatime, identifiers)
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(identified.Variants.Single().Barcode == "4950344872404" &&
            identified.Variants.Single().Sku == "TAM-1",
            "gtin13 wins over gtin and ean, and a numeric identifier keeps its digits");

        var onlyEan = await Scenario(Pasatime, identifiers.Replace("\"gtin13\":4950344872404,", string.Empty))
            .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(onlyEan.Variants.Single().Barcode == "00012345600012",
            "Without gtin13 the gtin field is the barcode and the ean is only the last resort");

        foreach (var (brand, expected) in new[]
        {
            ("\"brand\":\"Tamiya\",", "Tamiya"),
            ("\"brand\":{\"@type\":\"Brand\",\"name\":\"Tamiya\"},", "Tamiya"),
            ("\"brand\":[{\"@type\":\"Brand\",\"name\":\"Tamiya\"}],", "Tamiya"),
            ("\"brand\":\"\",", string.Empty)
        })
        {
            var sheet = await Scenario(Pasatime,
                    PasatimePage.Replace("\"brand\":{\"@type\":\"Brand\",\"name\":\"Tamiya\"},", brand))
                .Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
            check(sheet.Vendor == expected, "The brand becomes the vendor in every declared shape: " + brand);
        }

        var withoutCurrency = new StoreContext(Guid.NewGuid(), "Pasatime", "pasatime.cl",
            ProductSheetUrl.JsonLdConnectorKey, string.Empty, 20, 500);
        var declaredCurrency = await Scenario(withoutCurrency, PasatimePage)
            .Connector.LoadSheetAsync(withoutCurrency, PasatimeUrl, default);
        check(declaredCurrency.Currency == "CLP",
            "A store configured without currency falls back to the published priceCurrency");
        var noCurrencyAtAll = await Scenario(withoutCurrency,
                PasatimePage.Replace(",\"priceCurrency\":\"CLP\"", string.Empty))
            .Connector.LoadSheetAsync(withoutCurrency, PasatimeUrl, default);
        check(noCurrencyAtAll.Currency == "CLP",
            "With no currency anywhere the documented default applies instead of an empty code");
    }

    // La forma declarada de la ficha es parte de la regla compartida: mismo host sobre https y cualquier ruta.
    private static async Task SharedSheetShape(Action<bool, string> check)
    {
        foreach (var (store, candidate, accepted) in new (StoreContext, string, bool)[]
        {
            (Pasatime, PasatimeUrl, true),
            (Svader, SvaderUrl, true),
            (Titoys, TitoysUrl, true),
            (Pasatime, "https://pasatime.cl/producto/x?utm=1#foto", true),
            (Pasatime, "https://pasatime.cl/", false),
            (Pasatime, "https://pasatime.cl", false),
            (Pasatime, "http://pasatime.cl/producto/x/", false),
            (Pasatime, "https://evil.test/producto/x/", false),
            (Pasatime, "https://pasatime.cl.evil.test/producto/x/", false),
            (Pasatime, "https://user@pasatime.cl/producto/x/", false),
            (Pasatime, "https://pasatime.cl:444/producto/x/", false),
            (Pasatime, "https://pasatime.cl/a/../b", false),
            (Pasatime, "https://pasatime.cl/a\\b", false)
        })
        {
            check(ProductSheetUrl.IsProductSheet(store, candidate) == accepted,
                "The declared sheet shape answers from the store and the URL: " + candidate);
        }

        check(ProductSheetUrl.TryGetJsonLdSheetUrl(Pasatime, "https://www.pasatime.cl/producto/x/?q=1", out var canonical) &&
            canonical == "https://pasatime.cl/producto/x/",
            "The canonical sheet URL drops the query and keeps the published path");
        check(!ProductSheetUrl.TryGetHandle(Pasatime, PasatimeUrl, out _) &&
            !ProductSheetUrl.TryGetJsonLdSheetUrl(Store, PasatimeUrl, out _),
            "Each platform declares only its own shape: a JSON-LD store has no Shopify handle and vice versa");
        check(!ProductSheetUrl.IsProductSheet(
                new StoreContext(Guid.NewGuid(), "Koneko", "koneko.cl", "html-selectors", "CLP", 20, 500),
                "https://koneko.cl/producto/x"),
            "A platform that declares no shape still accepts no URL");

        var scenario = Scenario(Pasatime, PasatimePage);
        await RejectAsync(() => scenario.Connector.LoadSheetAsync(Pasatime, "https://evil.test/producto/x", default),
            "Introduce una URL válida de Pasatime: https://pasatime.cl/ruta-del-producto");
        check(scenario.Transport.Uris.Count == 0,
            "A URL of another host is refused before robots.txt and before the sheet");
    }

    // (e) El descubrimiento parte del sitemap de la propia tienda: se prefiere el documento cuyo nombre contiene
    // "product" y, si el sitemap es una lista plana, esa lista sea el catalogo.
    private static async Task Discovery(Action<bool, string> check)
    {
        const string product1 = "https://pasatime.cl/product-sitemap1.xml";
        const string product2 = "https://pasatime.cl/product-sitemap2.xml";
        const string pages = "https://pasatime.cl/page-sitemap.xml";
        const string first = "https://pasatime.cl/producto/tamiya-cinta/";
        const string second = "https://pasatime.cl/producto/bandai-rg/";
        const string third = "https://pasatime.cl/producto/tamiya-pincel/";

        var index = DiscoveryScenario(Pasatime,
            index: Index(pages, product1, product2),
            documents: new Dictionary<string, string>
            {
                [product1] = UrlSet(first, second),
                [pages] = UrlSet("https://pasatime.cl/contacto"),
                [product2] = UrlSet(third)
            });
        var walk = await index.Discovery.DiscoverAsync(Pasatime, new RequestBudget(50), default);
        check(walk.ProductUrls.SequenceEqual(new[] { first, second, third }),
            "Pasatime queues the product sitemap URLs in document order: " + string.Join(", ", walk.ProductUrls));
        check(walk.CatalogSkipped.Select(skip => (skip.Reference, skip.Reason)).SequenceEqual(new[]
            {
                (pages, CatalogSkipReason.NotProductSitemap)
            }),
            "A document whose name does not contain product is recorded instead of walked");
        check(index.Transport.Uris.SequenceEqual(new[]
            {
                "https://pasatime.cl/robots.txt", "https://pasatime.cl/sitemap.xml", product1, product2
            }),
            "Only the index and the product documents are requested: " + string.Join(", ", index.Transport.Uris));
        check(walk.DiscoveredCount == 3 && walk.BudgetConsumed == 6,
            "The walk reports three queued URLs and spends the index, two documents and three reservations");

        const string nested = "https://svadercoleccionables.cl/dv8-1/2-comic-en-ingles";
        const string flat = "https://svadercoleccionables.cl/producto/manga-nuevo";
        var flatSitemap = DiscoveryScenario(Svader,
            index: UrlSet("https://svadercoleccionables.cl/", "https://svadercoleccionables.cl/categoria/mangas", nested, flat),
            documents: []);
        var flatWalk = await flatSitemap.Discovery.DiscoverAsync(Svader, new RequestBudget(50), default);
        check(flatWalk.ProductUrls.SequenceEqual(new[] { "https://svadercoleccionables.cl/categoria/mangas", nested, flat }),
            "A flat sitemap is the catalogue and keeps its order: " + string.Join(", ", flatWalk.ProductUrls));
        check(flatWalk.ProductSkipped.SingleOrDefault() is
                { Reference: "https://svadercoleccionables.cl/", Reason: CatalogSkipReason.NotProductUrl },
            "The store root is not a product sheet and is recorded as such");
        check(flatWalk.DiscoveredCount == flatWalk.ProductUrls.Count,
            "The discovered counter describes what was queued, not every listed entry");

        var budgeted = DiscoveryScenario(Pasatime, index: Index(product1),
            documents: new Dictionary<string, string> { [product1] = UrlSet(first, second, third) });
        var budget = new RequestBudget(4);
        var limited = await budgeted.Discovery.DiscoverAsync(Pasatime, budget, default);
        check(limited.ProductUrls.Count == 2 && limited.BudgetConsumed == 4 &&
            limited.CatalogSkipped.Any(skip => skip.Reason == CatalogSkipReason.BudgetExhausted),
            $"Discovery queues exactly what the budget of 4 allows and records the stop: {limited.ProductUrls.Count}");
        check(budgeted.Transport.Uris.Count(uri => !uri.EndsWith("/robots.txt", StringComparison.Ordinal)) <= 4,
            "Discovery never exceeds the run budget, not even on a JSON-LD store");

        var tooSmall = DiscoveryScenario(Pasatime, index: Index(product1),
            documents: new Dictionary<string, string> { [product1] = UrlSet(first) });
        var empty = await tooSmall.Discovery.DiscoverAsync(Pasatime, new RequestBudget(2), default);
        check(empty.ProductUrls.Count == 0 &&
            tooSmall.Transport.Uris.SequenceEqual(new[] { "https://pasatime.cl/robots.txt", "https://pasatime.cl/sitemap.xml" }) &&
            empty.CatalogSkipped.SingleOrDefault() is { Reason: CatalogSkipReason.BudgetExhausted },
            "A budget that cannot pay for a document plus one sheet is recorded, not overspent");
    }

    // (f) El portal de robots se obedece igual que en Shopify: la ruta excluida no se pide, ni la ficha ni el sitemap.
    private static async Task RobotsGate(Action<bool, string> check)
    {
        var denied = Scenario(Pasatime, PasatimePage,
            "User-agent: *\nDisallow: /producto/tamiya-40mm-cinta-de-enmascarar-azul/\n");
        await RejectAsync(() => denied.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
            "robots.txt de pasatime.cl excluye /producto/tamiya-40mm-cinta-de-enmascarar-azul/: no se consultó la ficha de Pasatime.");
        check(denied.Transport.Uris.SequenceEqual(new[] { "https://pasatime.cl/robots.txt" }),
            "A disallowed sheet path is refused without requesting the page");

        var cached = Scenario(Pasatime, PasatimePage);
        await cached.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        await cached.Connector.LoadSheetAsync(Pasatime, "https://pasatime.cl/producto/otro/", default);
        check(cached.Transport.Uris.Count(uri => uri.EndsWith("/robots.txt", StringComparison.Ordinal)) == 1 &&
            cached.Transport.Uris.Count == 3,
            "robots.txt is fetched once per host and reused by the JSON-LD connector");

        var forbidden = Scenario(Pasatime, PasatimePage, robots: null, robotsStatus: HttpStatusCode.Forbidden);
        await RejectAsync(() => forbidden.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default), null);
        check(forbidden.Transport.Uris.Count == 1,
            "An unreadable robots.txt is a prohibition, not permission");

        var noSitemap = DiscoveryScenario(Pasatime, index: Index("https://pasatime.cl/product-sitemap1.xml"),
            documents: new Dictionary<string, string>
            {
                ["https://pasatime.cl/product-sitemap1.xml"] = UrlSet("https://pasatime.cl/producto/oculto/")
            }, robots: "User-agent: *\nDisallow: /sitemap.xml\nDisallow: /producto/oculto/\n");
        var hidden = await noSitemap.Discovery.DiscoverAsync(Pasatime, new RequestBudget(20), default);
        check(hidden.ProductUrls.Count == 0 &&
            hidden.CatalogSkipped.SingleOrDefault() is
                { Reference: "https://pasatime.cl/sitemap.xml", Reason: CatalogSkipReason.RobotsDisallowed } &&
            noSitemap.Transport.Uris.SequenceEqual(new[] { "https://pasatime.cl/robots.txt" }),
            "A disallowed sitemap stops the walk without any further request");

        var hiddenProduct = DiscoveryScenario(Pasatime, index: UrlSet(
                "https://pasatime.cl/producto/visible/", "https://pasatime.cl/producto/oculto/"),
            documents: [], robots: "User-agent: *\nDisallow: /producto/oculto/\n");
        var partial = await hiddenProduct.Discovery.DiscoverAsync(Pasatime, new RequestBudget(20), default);
        check(partial.ProductUrls.SequenceEqual(new[] { "https://pasatime.cl/producto/visible/" }) &&
            partial.ProductSkipped.SingleOrDefault() is
                { Reference: "https://pasatime.cl/producto/oculto/", Reason: CatalogSkipReason.RobotsDisallowed },
            "A disallowed product path is skipped and recorded, and the allowed one is queued");
    }

    // (a), (e) y (f) Redirecciones de una tienda con JSON-LD: el 301 del propio host se sigue y el documento se lee,
    // cada salto se valida contra robots.txt y contra la forma de la ficha, y cada salto cuenta como una petición más
    // del limitador y del presupuesto.
    private static async Task Redirects(Action<bool, string> check)
    {
        const string hosted = "https://www.pasatime.cl/producto/tamiya-40mm-cinta-de-enmascarar-azul/";
        var followed = Scenario(Pasatime, PasatimePage, route: request =>
            request.RequestUri!.AbsoluteUri == PasatimeUrl
                ? Redirect(HttpStatusCode.MovedPermanently, hosted)
                : null);
        var sheet = await followed.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default);
        check(sheet.Title == "Tamiya 40mm Cinta de Enmascarar (Azul) • TAM87240 • Pasatime" &&
            sheet.Variants.Single().Price == 6390m && sheet.SourceUrl == PasatimeUrl,
            "A 301 to the same host over https is followed and the sheet is read with the requested source URL");
        check(followed.Transport.Uris.SequenceEqual(new[]
            {
                "https://pasatime.cl/robots.txt", PasatimeUrl, hosted
            }),
            "The followed hop is exactly one more request against the store host: " +
            string.Join(", ", followed.Transport.Uris));
        check(followed.Transport.MaxConcurrency == 1,
            "A hop counts as a request and stays one at a time per store host");

        const string hidden = "https://pasatime.cl/producto/destino-excluido/";
        var denied = Scenario(Pasatime, PasatimePage, "User-agent: *\nDisallow: /producto/destino-excluido/\n",
            route: request => request.RequestUri!.AbsoluteUri == PasatimeUrl
                ? Redirect(HttpStatusCode.MovedPermanently, hidden)
                : null);
        await RejectAsync(() => denied.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
            "robots.txt de pasatime.cl excluye /producto/destino-excluido/: no se consultó la ficha de Pasatime.");
        check(denied.Transport.Uris.SequenceEqual(new[] { "https://pasatime.cl/robots.txt", PasatimeUrl }),
            "robots.txt is consulted for the redirected path and the excluded target is never requested");

        var storefront = Scenario(Pasatime, PasatimePage,
            route: _ => Redirect(HttpStatusCode.Found, "https://pasatime.cl/"));
        await RejectAsync(() => storefront.Connector.LoadSheetAsync(Pasatime, PasatimeUrl, default),
            "La tienda redirigió la ficha de Pasatime a una ruta que no es una ficha de producto (https://pasatime.cl/) y la ingesta no la sigue.");
        check(storefront.Transport.Uris.Count == 2,
            "A redirect that leaves the sheet shape is refused without a second request");

        const string product1 = "https://pasatime.cl/product-sitemap1.xml";
        const string first = "https://pasatime.cl/producto/tamiya-cinta/";
        const string second = "https://pasatime.cl/producto/bandai-rg/";
        Dictionary<string, string> redirects = new()
        {
            ["https://pasatime.cl/sitemap.xml"] = "/sitemap_index.xml"
        };
        Dictionary<string, string> documents = new()
        {
            ["https://pasatime.cl/sitemap_index.xml"] = Index(product1),
            [product1] = UrlSet(first, second)
        };

        var walk = DiscoveryScenario(Pasatime, index: "", documents: documents, redirects: redirects);
        var result = await walk.Discovery.DiscoverAsync(Pasatime, new RequestBudget(20), default);
        check(result.ProductUrls.SequenceEqual(new[] { first, second }),
            "sitemap.xml answering 301 to sitemap_index.xml is followed and the catalogue is discovered: " +
            string.Join(", ", result.ProductUrls));
        check(walk.Transport.Uris.SequenceEqual(new[]
            {
                "https://pasatime.cl/robots.txt", "https://pasatime.cl/sitemap.xml",
                "https://pasatime.cl/sitemap_index.xml", product1
            }),
            "The hop is one more request, serialised by the store limiter: " +
            string.Join(", ", walk.Transport.Uris));
        check(result.BudgetConsumed == 5 && walk.Transport.MaxConcurrency == 1,
            "The hop is paid by the run budget like any other request: " + result.BudgetConsumed);

        var tight = DiscoveryScenario(Pasatime, index: "", documents: documents, redirects: redirects);
        var limited = await tight.Discovery.DiscoverAsync(Pasatime, new RequestBudget(2), default);
        check(limited.ProductUrls.Count == 0 && limited.BudgetConsumed == 2 &&
            limited.CatalogSkipped.Any(skip => skip.Reason == CatalogSkipReason.BudgetExhausted) &&
            tight.Transport.Uris.Count == 3,
            "With a budget of 2 the index and its hop are paid and the document is recorded as exhausted, not requested");

        var single = DiscoveryScenario(Pasatime, index: "", documents: documents, redirects: redirects);
        var unpaid = await single.Discovery.DiscoverAsync(Pasatime, new RequestBudget(1), default);
        check(unpaid.BudgetConsumed == 1 &&
            unpaid.CatalogSkipped.SingleOrDefault() is
                { Reference: "https://pasatime.cl/sitemap.xml", Reason: CatalogSkipReason.BudgetExhausted } &&
            single.Transport.Uris.SequenceEqual(new[]
                { "https://pasatime.cl/robots.txt", "https://pasatime.cl/sitemap.xml" }),
            "A hop the budget cannot pay stops the read and is recorded, never requested");

        var excluded = DiscoveryScenario(Pasatime, index: "", documents: [],
            robots: "User-agent: *\nDisallow: /sitemaps-excluidos.xml\n",
            redirects: new Dictionary<string, string>
            {
                ["https://pasatime.cl/sitemap.xml"] = "/sitemaps-excluidos.xml"
            });
        var refused = await excluded.Discovery.DiscoverAsync(Pasatime, new RequestBudget(20), default);
        check(refused.ProductUrls.Count == 0 &&
            refused.CatalogSkipped.SingleOrDefault() is
            {
                Reference: "https://pasatime.cl/sitemaps-excluidos.xml",
                Reason: CatalogSkipReason.RobotsDisallowed
            } &&
            excluded.Transport.Uris.SequenceEqual(new[]
                { "https://pasatime.cl/robots.txt", "https://pasatime.cl/sitemap.xml" }),
            "robots.txt is consulted for the redirected sitemap path and an excluded target is never requested");

        var foreign = DiscoveryScenario(Pasatime, index: "", documents: [],
            redirects: new Dictionary<string, string>
            {
                ["https://pasatime.cl/sitemap.xml"] = "https://evil.test/sitemap_index.xml"
            });
        var escaped = await foreign.Discovery.DiscoverAsync(Pasatime, new RequestBudget(20), default);
        check(escaped.ProductUrls.Count == 0 &&
            escaped.CatalogSkipped.SingleOrDefault() is
                { Reference: "https://pasatime.cl/sitemap.xml", Reason: CatalogSkipReason.SitemapUnavailable } &&
            foreign.Transport.Uris.Count == 2,
            "A sitemap redirect outside the store is recorded as unreadable and never followed");
    }

    private static StoreContext Store { get; } =
        new(Guid.NewGuid(), "Pasatime", "pasatime.cl", ShopifyConnector.ConnectorKey, "CLP", 20, 500);

    private static StoreContext Pasatime { get; } =
        new(Guid.NewGuid(), "Pasatime", "pasatime.cl", ProductSheetUrl.JsonLdConnectorKey, "CLP", 20, 500);

    private static StoreContext Svader { get; } =
        new(Guid.NewGuid(), "Svader", "svadercoleccionables.cl", ProductSheetUrl.JsonLdConnectorKey, "CLP", 20, 500);

    private static StoreContext Titoys { get; } =
        new(Guid.NewGuid(), "Titoys", "titoys.cl", ProductSheetUrl.JsonLdConnectorKey, "CLP", 20, 500);

    private static StoreContext Otakuverso { get; } =
        new(Guid.NewGuid(), "Otakuverso", "otakuverso.cl", ProductSheetUrl.JsonLdConnectorKey, "CLP", 20, 500);

    private static Sheet Scenario(StoreContext store, string body, string? robots = PermissiveRobots,
        HttpStatusCode robotsStatus = HttpStatusCode.OK,
        Func<HttpRequestMessage, HttpResponseMessage?>? route = null)
    {
        var transport = new FakeTransport
        {
            Respond = request => request.RequestUri!.AbsolutePath == "/robots.txt"
                ? robotsStatus == HttpStatusCode.OK
                    ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(robots ?? "") }
                    : new HttpResponseMessage(robotsStatus)
                : route?.Invoke(request) ??
                    new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) }
        };
        var http = new HttpClient(transport);
        var limiter = new DomainRequestLimiter((_, _) => Task.CompletedTask);
        return new Sheet(transport, new JsonLdConnector(http, new RobotsTxtCache(http, limiter), limiter));
    }

    // El doble de transporte devuelve la redirección pero no la sigue, igual que el transporte real de la ingesta.
    private static HttpResponseMessage Redirect(HttpStatusCode status, string location)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
        return response;
    }

    private static Walk DiscoveryScenario(StoreContext store, string index,
        Dictionary<string, string> documents, string? robots = PermissiveRobots,
        Dictionary<string, string>? redirects = null)
    {
        var transport = new FakeTransport();
        transport.Respond = request =>
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path == "/robots.txt")
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(robots ?? "") };
            var uri = request.RequestUri.AbsoluteUri;
            if (redirects is not null && redirects.TryGetValue(uri, out var location))
                return Redirect(HttpStatusCode.MovedPermanently, location);
            if (uri == $"https://{store.Host}/sitemap.xml")
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(index) };
            return documents.TryGetValue(uri, out var body)
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

    // Cambia solo la disponibilidad de la ficha de pasatime; el resto del documento queda como lo publica la tienda.
    // El campo ausente y el campo nulo significan lo mismo para el conector, pero aquí se borra para no confundir.
    private static string WithAvailability(string body, string? availability) => availability is null
        ? body.Replace("\"availability\":\"https://schema.org/InStock\",", string.Empty)
        : body.Replace("\"availability\":\"https://schema.org/InStock\"", $"\"availability\":\"{availability}\"");

    // Cambia solo el precio (valor JSON ya listo) de la ficha de pasatime.
    private static string WithPrice(string body, string price) =>
        body.Replace("\"price\":\"6390 CLP\"", "\"price\":" + price);

    private const string PasatimePage = """
        <!doctype html>
        <html lang="es-CL"><head><meta charset="utf-8">
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"Product",
        "name":"Tamiya 40mm Cinta de Enmascarar (Azul) • TAM87240 • Pasatime",
        "sku":"TAM87240","gtin13":"4950344872404","brand":{"@type":"Brand","name":"Tamiya"},
        "category":"Modelismo","description":"Cinta de enmascarar de 40mm.",
        "image":["https://pasatime.cl/img/tamiya-87240.jpg"],
        "offers":{"@type":"Offer","price":"6390 CLP","priceCurrency":"CLP",
        "availability":"https://schema.org/InStock","url":"https://pasatime.cl/producto/tamiya-40mm-cinta-de-enmascarar-azul/"}}
        </script>
        </head><body><h1>Tamiya 40mm Cinta de Enmascarar (Azul)</h1></body></html>
        """;

    private const string SvaderPage = """
        <!doctype html>
        <html><head>
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"Product","name":"Dv8 #1/2",
        "offers":[{"@type":"Offer","price":1990.0,"priceCurrency":"CLP",
        "availability":"https://schema.org/InStock"}]}
        </script>
        </head><body>Dv8 #1/2</body></html>
        """;

    private const string TitoysPage = """
        <!doctype html>
        <html><head>
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"Product",
        "name":"CAJA COMPLETA - Auto de Juguete Colo-Colo Escala 1:64","sku":"TDC01",
        "offers":{"@type":"Offer","price":108000.0,"priceCurrency":"CLP",
        "availability":"https://schema.org/InStock"}}
        </script>
        </head><body>Auto Colo-Colo</body></html>
        """;

    // Un listado publica un nodo Product por tarjeta, cada uno con la URL de su ficha.
    private const string OtakuversoPage = """
        <!doctype html>
        <html><head>
        <script type="application/ld+json">
        {"@context":"https://schema.org","@graph":[
        {"@type":"Product","name":"Mazinger Z Infinity","url":"https://otakuverso.cl/productos/mazinger-z-infinity",
        "offers":{"@type":"Offer","price":6990,"priceCurrency":"CLP","availability":"https://schema.org/InStock"}},
        {"@type":"Product","name":"Evangelion 01","url":"https://otakuverso.cl/productos/evangelion-01",
        "offers":{"@type":"Offer","price":12990,"priceCurrency":"CLP","availability":"https://schema.org/InStock"}},
        {"@type":"Product","name":"Gundam Exia","url":"https://otakuverso.cl/productos/gundam-exia",
        "offers":{"@type":"Offer","price":8990,"priceCurrency":"CLP","availability":"https://schema.org/OutOfStock"}}]}
        </script>
        </head><body><a href="/productos/mazinger-z-infinity">Mazinger Z Infinity</a></body></html>
        """;

    private const string CategoryPage = """
        <!doctype html>
        <html><head>
        <script type="application/ld+json">
        {"@context":"https://schema.org","@type":"BreadcrumbList","itemListElement":[
        {"@type":"ListItem","position":1,"name":"Inicio","item":"https://pasatime.cl/"},
        {"@type":"ListItem","position":2,"name":"Maquetas","item":"https://pasatime.cl/categoria/maquetas"}]}
        </script>
        </head><body><h1>Maquetas</h1></body></html>
        """;

    private static async Task RejectAsync(Func<Task> action, string? message)
    {
        try
        {
            await action();
        }
        catch (Exception exception) when (exception is StoreOfferUrlException or StorePathDisallowedException
            or StoreRedirectException or InvalidDataException or HttpRequestException)
        {
            if (message is not null && exception.Message != message)
                throw new Exception($"Unexpected message: {exception.Message}");
            return;
        }

        throw new Exception("Expected rejection: " + (message ?? "any behaviour-level failure"));
    }

    private sealed record Sheet(FakeTransport Transport, JsonLdConnector Connector);

    private sealed record Walk(FakeTransport Transport, SitemapCatalogDiscovery Discovery);
}
