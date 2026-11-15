using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Maintainer.Inventories.IrionOffers;

public record IrionOfferSnapshot(string Currency, decimal Price, decimal? OriginalPrice, bool Available);

// Non-saved outcomes carry data even though the outer ApiResponse.Success is false.
public record IrionOfferConfirmationDto(
    string Outcome,
    Guid? InventoryId,
    IrionProductPreviewDto? FreshPreview = null,
    IrionOfferInspectionDto? Inspection = null);
