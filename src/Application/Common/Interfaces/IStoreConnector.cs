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
}
