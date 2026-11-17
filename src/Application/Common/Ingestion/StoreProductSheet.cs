using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Common.Ingestion;

/// <summary>
/// Ficha de tienda tal como la entrega un conector. Añade a la vista previa el tipo de producto que publica la
/// plataforma (Shopify lo expone como <c>type</c>), porque la clasificación de la ingesta lo necesita antes que las
/// etiquetas o el título. La vista previa manual sigue publicando el mismo DTO de siempre: aquí no se cambia ningún
/// contrato HTTP.
/// </summary>
public sealed record StoreProductSheet(
    string SourceUrl,
    string Title,
    string Vendor,
    string Currency,
    string DescriptionHtml,
    string? ProductType,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> ImageUrls,
    bool Available,
    IReadOnlyList<IrionVariantPreviewDto> Variants)
{
    // El campo vacío del tipo de producto significa "no informado", igual que en los identificadores de la ficha.
    public string? DeclaredProductType => string.IsNullOrWhiteSpace(ProductType) ? null : ProductType.Trim();

    public IrionProductPreviewDto ToPreview() =>
        new(SourceUrl, Title, Vendor, Currency, DescriptionHtml, Tags, ImageUrls, Available, Variants);

    // Los conectores que todavía no publican tipo de producto (y los dobles de las comprobaciones) siguen entrando
    // por la vista previa: el tipo queda como no informado en vez de inventarse.
    public static StoreProductSheet FromPreview(IrionProductPreviewDto preview) =>
        new(preview.SourceUrl, preview.Title, preview.Vendor, preview.Currency, preview.DescriptionHtml, null,
            preview.Tags, preview.ImageUrls, preview.Available, preview.Variants);
}
