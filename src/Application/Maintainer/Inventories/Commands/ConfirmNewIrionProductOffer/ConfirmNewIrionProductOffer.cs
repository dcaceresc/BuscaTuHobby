using Application.Common.Exceptions;
using Application.Common.PriceHistory;
using Application.Maintainer.Inventories.IrionOffers;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Maintainer.Inventories.Commands.ConfirmNewIrionProductOffer;

public record ConfirmNewIrionProductOffer(
    NewIrionProductDetails Product, Guid StoreId, string SourceUrl, string ExternalVariantId,
    Guid? ExpectedInventoryId, string? ExpectedRowVersion, IrionOfferSnapshot ExpectedOffer)
    : IRequest<ApiResponse<IrionOfferConfirmationDto>>;

public class ConfirmNewIrionProductOfferHandler(
    IApplicationDbContext context, IApiResponseService responseService, IIrionProductExtractor extractor)
    : IRequestHandler<ConfirmNewIrionProductOffer, ApiResponse<IrionOfferConfirmationDto>>
{
    public async Task<ApiResponse<IrionOfferConfirmationDto>> Handle(ConfirmNewIrionProductOffer request, CancellationToken cancellationToken)
    {
        IrionOfferInspectionDto? inspection = null;
        ApiResponse<IrionOfferConfirmationDto> Outcome(string outcome, Guid? id = null, IrionProductPreviewDto? preview = null)
        {
            var data = new IrionOfferConfirmationDto(outcome, id, preview, inspection);
            return outcome is "Saved" or "AlreadySaved" ? responseService.Success(data) :
                new ApiResponse<IrionOfferConfirmationDto> { Success = false, Data = data };
        }
        try
        {
            var initial = await NewIrionProductValidation.InspectAsync(context, request.Product, request.StoreId,
                request.SourceUrl, request.ExternalVariantId, cancellationToken);
            if (initial.Error is not null) return responseService.Fail<IrionOfferConfirmationDto>(initial.Error);
            inspection = initial.Inspection!;
            if (inspection.Outcome == "Conflict") return Outcome("Conflict", inspection.InventoryId);
            if ((request.ExpectedInventoryId is null && request.ExpectedRowVersion is not null) ||
                (request.ExpectedInventoryId is not null && !ValidVersion(request.ExpectedRowVersion)))
                return Outcome("Conflict", inspection.InventoryId);
            if (request.ExpectedOffer is null) throw new InvalidOperationException();

            var preview = await extractor.ExtractAsync(request.SourceUrl, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            // Track the product now, not before external I/O. Matching includes the complete metadata and category set.
            var current = await NewIrionProductValidation.InspectAsync(context, request.Product, request.StoreId,
                request.SourceUrl, request.ExternalVariantId, cancellationToken, trackProduct: true);
            if (current.Error is not null) return responseService.Fail<IrionOfferConfirmationDto>(current.Error);
            inspection = current.Inspection!;
            if (inspection.Outcome == "Conflict") return Outcome("Conflict", inspection.InventoryId);
            var variant = preview.Variants.SingleOrDefault(v => v.Id == request.ExternalVariantId);
            if (preview.SourceUrl != request.SourceUrl || variant is null ||
                request.ExpectedOffer != new IrionOfferSnapshot(preview.Currency, variant.Price, variant.OriginalPrice, variant.Available))
                return Outcome("RefreshRequired", inspection.InventoryId, preview);
            var original = variant.OriginalPrice ?? 0;
            if (preview.Currency != "CLP" || !Whole(variant.Price) || variant.Price <= 0 || !Whole(original) ||
                original < 0 || (original != 0 && original < variant.Price)) throw new InvalidOperationException();

            var d = request.Product;
            var row = await context.Inventories.SingleOrDefaultAsync(i => i.ProductId == d.NewProductId && i.StoreId == request.StoreId, cancellationToken);
            // Re-check using tracked product identity as well: a fresh no-tracking inspection alone must not authorize an occupied ID.
            var final = await NewIrionProductValidation.InspectAsync(context, d, request.StoreId, request.SourceUrl,
                request.ExternalVariantId, cancellationToken, trackProduct: true);
            if (final.Error is not null) return responseService.Fail<IrionOfferConfirmationDto>(final.Error);
            inspection = final.Inspection!;
            if (inspection.Outcome == "Conflict") return Outcome("Conflict", inspection.InventoryId);
            var price = (int)variant.Price;
            var reference = (int)original;
            var discount = reference == 0 ? 0 : (int)Math.Round((reference - (decimal)price) * 100 / reference, MidpointRounding.AwayFromZero);
            if (row is not null)
            {
                if (inspection.Outcome != "Update" || row.SourceUrl != request.SourceUrl || row.ExternalVariantId != request.ExternalVariantId ||
                    (request.ExpectedInventoryId is not null && request.ExpectedInventoryId != row.InventoryId))
                    return Outcome("Conflict", row.InventoryId);
                if (row.Price == price && row.OriginalPrice == reference && row.DiscountPercentage == discount && row.Availability == variant.Available)
                    return Outcome("AlreadySaved", row.InventoryId);
                if (request.ExpectedInventoryId != row.InventoryId || request.ExpectedRowVersion != Convert.ToBase64String(row.RowVersion))
                    return Outcome("Conflict", row.InventoryId);
                var before = InventoryPricePoint.Capture(row);
                // La importación solo refresca precios y disponibilidad: conserva la condición y el medio de pago ya registrados.
                row.UpdateImportedOffer(price, reference, discount, request.SourceUrl, request.ExternalVariantId,
                    variant.Available, DateTime.UtcNow, row.Condition, row.PaymentMethodLabel);
                InventoryPriceHistoryRecorder.Record(context, row, before);
            }
            else
            {
                if (inspection.Outcome != "Create" || request.ExpectedInventoryId is not null) return Outcome("Conflict");
                var product = Product.CreateWithProvidedId(d.NewProductId, d.ProductName, d.ManufacturerId, d.FranchiseId, d.SerieId,
                    d.ProductHasBase!.Value, d.ProductTargetAge, d.ProductSize, d.ProductDescription, d.ProductReleaseDate!.Value);
                row = Inventory.CreateImportedOffer(d.NewProductId, request.StoreId, price, reference, discount,
                    request.SourceUrl, request.ExternalVariantId, variant.Available, DateTime.UtcNow);
                context.Products.Add(product);
                foreach (var categoryId in d.CategoryIds) context.ProductCategories.Add(product.AssignCategory(categoryId));
                context.Inventories.Add(row);
                InventoryPriceHistoryRecorder.RecordBaseline(context, row);
            }
            // The normal relational SaveChanges transaction is the sole product/categories/offer persistence boundary.
            await context.SaveChangesAsync(cancellationToken);
            return Outcome("Saved", row.InventoryId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (DbUpdateException) { return Outcome("Conflict", inspection?.InventoryId); }
        catch (IrionProductUrlException e) { return responseService.Fail<IrionOfferConfirmationDto>(e.Message); }
        catch (IrionRateLimitException e) { return responseService.Fail<IrionOfferConfirmationDto>(e.Message); }
        catch (Exception) { return responseService.Fail<IrionOfferConfirmationDto>("No se pudo confirmar la oferta de Irion."); }
    }
    private static bool Whole(decimal value) => value >= 0 && value <= int.MaxValue && decimal.Truncate(value) == value;
    private static bool ValidVersion(string? version)
    {
        if (version is null) return false;
        Span<byte> bytes = stackalloc byte[8];
        return Convert.TryFromBase64String(version, bytes, out var length) && length == 8 && Convert.ToBase64String(bytes) == version;
    }
}
