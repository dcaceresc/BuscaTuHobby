namespace Application.Maintainer.Inventories.IrionOffers;

// Outcome is an explicit API contract, independent of translated explanatory messages.
public record IrionOfferInspectionDto(
    string Outcome,
    Guid ProductId,
    Guid StoreId,
    string SourceUrl,
    string ExternalVariantId,
    Guid? InventoryId,
    string? RowVersion,
    bool? IsActive,
    string? Message);
