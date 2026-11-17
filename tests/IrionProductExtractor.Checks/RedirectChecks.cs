using Application.Common.Ingestion;
using Application.Common.Interfaces;

// Decisión pura del salto de redirección: qué se sigue y qué se rechaza, sin red, sin transporte y sin base de datos.
// Es la regla que comparten el descubrimiento y los dos conectores, así que se comprueba una sola vez aquí.
internal static class RedirectChecks
{
    private const string Current = "https://irion.cl/sitemap.xml";

    public static void Run(Action<bool, string> check)
    {
        Follows(check);
        Refuses(check);
        SheetRule(check);
    }

    // (a) Se sigue dentro de la tienda: el mismo host con o sin "www", resuelto contra la URL que se estaba leyendo,
    // sin fragmento y conservando la consulta.
    private static void Follows(Action<bool, string> check)
    {
        var relative = RedirectTarget.Resolve(Store, Current, "/sitemap_index.xml", 0);
        check(relative.Allowed && relative.Target == "https://irion.cl/sitemap_index.xml" &&
            relative.Refusal == RedirectRefusal.None,
            "A same-host https redirect from a relative destination resolves and is followed: " + relative.Target);

        var hosted = RedirectTarget.Resolve(Store, Current, "https://www.irion.cl/sitemap_index.xml", 0);
        check(hosted.Allowed && hosted.Target == "https://www.irion.cl/sitemap_index.xml",
            "The \"www.\" prefix of the same store is not another host");

        var query = RedirectTarget.Resolve(Store, Current, "https://irion.cl/sitemap_index.xml?desde=301#top", 0);
        check(query.Allowed && query.Target == "https://irion.cl/sitemap_index.xml?desde=301",
            "The query survives the hop, because the store may need it, and the fragment does not travel: " + query.Target);

        var second = RedirectTarget.Resolve(Store, Current, "/sitemap_index.xml", 1);
        check(second.Allowed,
            "A second hop is still inside the bound of " + RedirectTarget.MaximumHops + " hops");
    }

    // (b), (c) y (d): otro host, otro esquema, otro puerto y una tercera redirección nunca se siguen.
    private static void Refuses(Action<bool, string> check)
    {
        var foreign = RedirectTarget.Resolve(Store, Current, "https://evil.test/sitemap_index.xml", 0);
        check(foreign.Refusal == RedirectRefusal.OutsideStore && !foreign.Allowed,
            "Another host is refused instead of followed");

        var lookalike = RedirectTarget.Resolve(Store, Current, "https://irion.cl.evil.test/sitemap_index.xml", 0);
        check(lookalike.Refusal == RedirectRefusal.OutsideStore,
            "A host that merely ends with the store host is another host");

        var insecure = RedirectTarget.Resolve(Store, Current, "http://irion.cl/sitemap_index.xml", 0);
        check(insecure.Refusal == RedirectRefusal.NotHttps, "A redirect to http is refused");

        var otherScheme = RedirectTarget.Resolve(Store, Current, "ftp://irion.cl/sitemap_index.xml", 0);
        check(otherScheme.Refusal == RedirectRefusal.NotHttps, "A redirect to another scheme is refused");

        var port = RedirectTarget.Resolve(Store, Current, "https://irion.cl:8443/sitemap_index.xml", 0);
        check(port.Refusal == RedirectRefusal.OutsideStore, "A destination on another port is another surface");

        var credentials = RedirectTarget.Resolve(Store, Current, "https://user@irion.cl/sitemap_index.xml", 0);
        check(credentials.Refusal == RedirectRefusal.OutsideStore,
            "A destination that carries credentials is another surface");

        var missing = RedirectTarget.Resolve(Store, Current, null, 0);
        check(missing.Refusal == RedirectRefusal.NoDestination && !missing.Allowed,
            "A redirect without a destination is refused instead of guessed");

        var blank = RedirectTarget.Resolve(Store, Current, "   ", 0);
        check(blank.Refusal == RedirectRefusal.NoDestination, "A blank destination is the same as no destination");

        var third = RedirectTarget.Resolve(Store, Current, "/sitemap_index.xml", RedirectTarget.MaximumHops);
        check(third.Refusal == RedirectRefusal.TooManyHops && !third.Allowed,
            "The third hop is refused even when its destination would be valid");
    }

    // La regla de la ficha se aplica al destino: sólo se sigue una redirección que siga siendo la lectura de una
    // ficha de esta tienda, con la forma que declara su plataforma.
    private static void SheetRule(Action<bool, string> check)
    {
        var accepted = RedirectTarget.Resolve(Store, Current, "https://irion.cl/products/otro.js", 0,
            url => ProductSheetUrl.IsSheetRead(Store, url));
        check(accepted.Allowed, "A redirect to another sheet of the same store is followed");

        var refused = RedirectTarget.Resolve(Store, Current, "https://irion.cl/admin.js", 0,
            url => ProductSheetUrl.IsSheetRead(Store, url));
        check(refused.Refusal == RedirectRefusal.NotASheet,
            "A redirect that leaves the sheet shape of the platform is refused");

        foreach (var (candidate, expected) in new[]
        {
            ("https://irion.cl/products/optimus-prime.js", true),
            ("https://irion.cl/collections/all/products/optimus-prime.js", true),
            ("https://irion.cl/products/optimus-prime", false),
            ("https://irion.cl/products/optimus-prime.js/", false),
            ("https://irion.cl/admin.js", false),
            ("https://irion.cl.evil.test/products/optimus-prime.js", false),
            ("http://irion.cl/products/optimus-prime.js", false),
            ("https://irion.cl:444/products/optimus-prime.js", false)
        })
        {
            check(ProductSheetUrl.IsSheetRead(Store, candidate) == expected,
                "The sheet read of a Shopify store answers from the store and the URL: " + candidate);
        }

        foreach (var (candidate, expected) in new[]
        {
            ("https://pasatime.cl/producto/tamiya/", true),
            ("https://pasatime.cl/", false),
            ("https://otakuverso.cl/productos/mazinger-z", false)
        })
        {
            check(ProductSheetUrl.IsSheetRead(JsonLd, candidate) == expected,
                "The sheet read of a JSON-LD store accepts any path of its own host but never the root: " + candidate);
        }

        var unknown = new StoreContext(Guid.NewGuid(), "Koneko", "koneko.cl", "html-selectors", "CLP", 20, 500);
        check(!ProductSheetUrl.IsSheetRead(unknown, "https://koneko.cl/producto/x"),
            "A platform that declares no sheet shape has no sheet read either");
    }

    private static StoreContext Store { get; } =
        new(Guid.NewGuid(), "Irion", "irion.cl", ProductSheetUrl.ShopifyConnectorKey, "CLP", 20, 500);

    private static StoreContext JsonLd { get; } =
        new(Guid.NewGuid(), "Pasatime", "pasatime.cl", ProductSheetUrl.JsonLdConnectorKey, "CLP", 20, 500);
}
