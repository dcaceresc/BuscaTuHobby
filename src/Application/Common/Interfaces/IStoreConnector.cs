using Application.Common.Ingestion;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Common.Interfaces;

/// <summary>
/// Conector por plataforma, no por tienda. Deliberadamente no conoce el descubrimiento: hoy la importación
/// manual entrega una URL y la fase de sitemap añadirá el recorrido sin cambiar esta firma.
/// </summary>
public interface IStoreConnector
{
    /// <summary>Clave de plataforma que la fila de la tienda declara en <c>Store.ConnectorKey</c>.</summary>
    string Key { get; }

    /// <summary>
    /// Lee una ficha de producto de la tienda indicada. La URL se valida contra el host de la tienda y la
    /// moneda del resultado sale de la configuración de esa tienda.
    /// </summary>
    Task<IrionProductPreviewDto> LoadProductAsync(
        StoreContext store, string url, CancellationToken cancellationToken);

    /// <summary>
    /// Lee la ficha completa, que incluye el tipo de producto que publica la plataforma. La ingesta lo necesita para
    /// clasificar el producto; el preview manual no lo publica porque su DTO es un contrato existente.
    /// </summary>
    /// <remarks>
    /// La implementación por defecto deja que un conector que solo sepa entregar la vista previa siga funcionando:
    /// la ficha se arma con el tipo "no informado" en vez de obligar a tocar todos los conectores y dobles a la vez.
    /// </remarks>
    async Task<StoreProductSheet> LoadSheetAsync(
        StoreContext store, string url, CancellationToken cancellationToken) =>
        StoreProductSheet.FromPreview(await LoadProductAsync(store, url, cancellationToken));
}
