using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Common.Interfaces;

public interface IIrionProductExtractor
{
    Task<IrionProductPreviewDto> ExtractAsync(string url, CancellationToken cancellationToken);
}
