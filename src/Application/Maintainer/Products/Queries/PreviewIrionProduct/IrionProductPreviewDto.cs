namespace Application.Maintainer.Products.Queries.PreviewIrionProduct;

public record IrionProductPreviewDto(
    string SourceUrl,
    string Title,
    string Vendor,
    string Currency,
    string DescriptionHtml,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> ImageUrls,
    bool Available,
    IReadOnlyList<IrionVariantPreviewDto> Variants);

public record IrionVariantPreviewDto(
    string Id,
    string? Sku,
    string? Barcode,
    bool Available,
    decimal Price,
    decimal? OriginalPrice,
    decimal? DiscountPercentage);
