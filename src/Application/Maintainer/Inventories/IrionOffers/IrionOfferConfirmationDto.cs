using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Maintainer.Inventories.IrionOffers;

public record IrionOfferSnapshot(string Currency, decimal Price, decimal? OriginalPrice, bool Available);

// Los resultados no guardados llevan datos aunque el ApiResponse.Success exterior sea false.
public record IrionOfferConfirmationDto(
    string Outcome,
    Guid? InventoryId,
    IrionProductPreviewDto? FreshPreview = null,
    IrionOfferInspectionDto? Inspection = null);
