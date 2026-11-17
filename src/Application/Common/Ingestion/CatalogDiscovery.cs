namespace Application.Common.Ingestion;

// Motivo por el que una entrada del sitemap no se encoló. Se guarda como texto explicable, no como un booleano:
// "no se encoló" sin decir por qué no sirve para auditar una ingesta.
public enum CatalogSkipReason
{
    NotProductSitemap = 0,
    RobotsDisallowed = 1,
    SitemapUnavailable = 2,
    NotProductUrl = 3,
    AlreadyStored = 4,
    BudgetExhausted = 5
}

// Una entrada descartada durante el recorrido. La referencia es lo que identifica lo descartado: la URL del
// documento del sitemap, la de la ficha de producto o el documento donde se agotó el presupuesto.
public sealed record CatalogSkip(string Reference, CatalogSkipReason Reason)
{
    // El texto se escribe una sola vez para que el registro del descarte y el error del elemento coincidan.
    public string Message => Reason switch
    {
        CatalogSkipReason.NotProductSitemap => NotProductSitemapMessage,
        CatalogSkipReason.RobotsDisallowed => RobotsDisallowedMessage,
        CatalogSkipReason.SitemapUnavailable => SitemapUnavailableMessage,
        CatalogSkipReason.NotProductUrl => NotProductUrlMessage,
        CatalogSkipReason.AlreadyStored => AlreadyStoredMessage,
        _ => BudgetExhaustedMessage
    };

    public const string NotProductSitemapMessage =
        "El documento no es un sitemap de productos: sólo se siguen los sitemap_products_*.";

    public const string RobotsDisallowedMessage =
        "robots.txt prohíbe esta ruta: no se consultó.";

    public const string SitemapUnavailableMessage =
        "El sitemap no se pudo leer: la ejecución continúa con el siguiente documento.";

    public const string NotProductUrlMessage =
        "La entrada no es una ficha de producto de esta tienda.";

    public const string AlreadyStoredMessage =
        "Una ejecución completada anterior ya guardó esta URL.";

    public const string BudgetExhaustedMessage =
        "El presupuesto de peticiones de la ejecución se agotó antes de terminar el recorrido.";
}

// Resultado de un recorrido de catálogo. Separa lo descartado por documento de lo descartado por entrada porque
// sólo lo segundo es una URL de producto y, por lo tanto, lo único que se puede auditar como elemento.
// DiscoveredCount cuenta lo encolado (ya deduplicado y validado por la regla de ficha), no lo leído: un contador que
// suma descartes deja de describir el trabajo que la ejecución tiene por delante.
public sealed record CatalogDiscoveryResult(
    IReadOnlyList<string> ProductUrls,
    IReadOnlyList<CatalogSkip> CatalogSkipped,
    IReadOnlyList<CatalogSkip> ProductSkipped,
    int DiscoveredCount,
    int BudgetConsumed)
{
    public static CatalogDiscoveryResult Empty { get; } = new([], [], [], 0, 0);
}
