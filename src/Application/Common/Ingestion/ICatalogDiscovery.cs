namespace Application.Common.Ingestion;

/// <summary>
/// Descubrimiento del catálogo de una tienda configurada. No adivina rutas: parte del <c>sitemap.xml</c> que la
/// tienda publica y devuelve las URL de ficha que encontró, junto con lo que descartó y por qué.
/// </summary>
public interface ICatalogDiscovery
{
    Task<CatalogDiscoveryResult> DiscoverAsync(
        StoreContext store, RequestBudget budget, CancellationToken cancellationToken);
}
