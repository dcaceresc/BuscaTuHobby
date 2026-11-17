using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Common.Interfaces;

/// <summary>
/// Punto de entrada que usan los handlers de importación manual: reciben una URL, resuelven la tienda desde su
/// fila de configuración y delegan en el conector de esa plataforma. No hay ninguna tienda especial en código.
/// </summary>
public interface IStoreProductLoader
{
    Task<IrionProductPreviewDto> LoadAsync(string url, CancellationToken cancellationToken);
}
