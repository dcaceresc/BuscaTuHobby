using Application.Common.Exceptions;
using Application.Common.PriceHistory;
using Application.Maintainer.Inventories.IrionOffers;
using Application.Maintainer.Inventories.Queries.InspectIrionOffer;
using Application.Maintainer.Products.Queries.PreviewIrionProduct;

namespace Application.Maintainer.Inventories.Commands.ConfirmIrionOffer;

public record ConfirmIrionOffer(
    Guid ProductId, Guid StoreId, string SourceUrl, string ExternalVariantId,
    Guid? ExpectedInventoryId, string? ExpectedRowVersion, IrionOfferSnapshot ExpectedOffer)
    : IRequest<ApiResponse<IrionOfferConfirmationDto>>;

public class ConfirmIrionOfferHandler(
    IApplicationDbContext context,
    IApiResponseService responseService,
    IIrionProductExtractor extractor,
    IRequestHandler<InspectIrionOffer, ApiResponse<IrionOfferInspectionDto>> inspector)
    : IRequestHandler<ConfirmIrionOffer, ApiResponse<IrionOfferConfirmationDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IIrionProductExtractor _extractor = extractor;
    private readonly IRequestHandler<InspectIrionOffer, ApiResponse<IrionOfferInspectionDto>> _inspector = inspector;

    public async Task<ApiResponse<IrionOfferConfirmationDto>> Handle(ConfirmIrionOffer request, CancellationToken cancellationToken)
    {
        IrionOfferInspectionDto? inspection = null;
        ApiResponse<IrionOfferConfirmationDto> Outcome(string outcome, Guid? id = null,
            IrionProductPreviewDto? preview = null)
        {
            var data = new IrionOfferConfirmationDto(outcome, id, preview, inspection);
            if (outcome is "Saved" or "AlreadySaved") return _responseService.Success(data);
            // Un no-éxito tipado es distinto a propósito de los errores de validación o extracción (data:null).
            return new ApiResponse<IrionOfferConfirmationDto> { Success = false, Data = data };
        }
        try
        {
            var selection = new InspectIrionOffer(request.ProductId, request.StoreId, request.SourceUrl, request.ExternalVariantId);
            var initial = await _inspector.Handle(selection, cancellationToken);
            if (!initial.Success) return _responseService.Fail<IrionOfferConfirmationDto>(initial.Message!);
            inspection = initial.Data!;
            if (inspection.Outcome == "Conflict") return Outcome("Conflict", inspection.InventoryId);
            if ((request.ExpectedInventoryId is null && request.ExpectedRowVersion is not null) ||
                (request.ExpectedInventoryId is not null && !ValidVersion(request.ExpectedRowVersion)))
                return Outcome("Conflict", inspection.InventoryId);
            if (request.ExpectedOffer is null) return _responseService.Fail<IrionOfferConfirmationDto>("No se pudo confirmar la oferta de Irion.");

            // Una sola extracción segura, nunca un reintento, un respaldo ni una escritura enviada por el navegador.
            var preview = await _extractor.ExtractAsync(request.SourceUrl, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var current = await _inspector.Handle(selection, cancellationToken);
            if (!current.Success) return _responseService.Fail<IrionOfferConfirmationDto>(current.Message!);
            inspection = current.Data!;
            if (inspection.Outcome == "Conflict") return Outcome("Conflict", inspection.InventoryId);
            if (preview.SourceUrl != request.SourceUrl) throw new InvalidOperationException();
            var variant = preview.Variants.SingleOrDefault(v => v.Id == request.ExternalVariantId);
            if (variant is null) return Outcome("RefreshRequired", inspection.InventoryId, preview);
            var original = variant.OriginalPrice ?? 0;
            if (preview.Currency != "CLP" || !Whole(variant.Price) || variant.Price <= 0 || !Whole(original) ||
                original < 0 || (original != 0 && original < variant.Price))
                throw new InvalidOperationException();
            if (request.ExpectedOffer != new IrionOfferSnapshot(preview.Currency, variant.Price, variant.OriginalPrice, variant.Available))
                return Outcome("RefreshRequired", inspection.InventoryId, preview);

            // El seguimiento empieza después de la E/S externa. EF conserva este rowversion como predicado del UPDATE.
            var row = await _context.Inventories.SingleOrDefaultAsync(
                i => i.ProductId == request.ProductId && i.StoreId == request.StoreId, cancellationToken);
            var price = (int)variant.Price;
            var reference = (int)original;
            var discount = reference == 0 ? 0 : (int)Math.Round((reference - (decimal)price) * 100 / reference,
                MidpointRounding.AwayFromZero);
            if (row is not null)
            {
                if (row.SourceUrl != request.SourceUrl || row.ExternalVariantId != request.ExternalVariantId ||
                    (request.ExpectedInventoryId is not null && request.ExpectedInventoryId != row.InventoryId))
                    return Outcome("Conflict", row.InventoryId);
                if (row.Price == price && row.OriginalPrice == reference && row.DiscountPercentage == discount &&
                    row.Availability == variant.Available)
                    return Outcome("AlreadySaved", row.InventoryId);
                if (request.ExpectedInventoryId != row.InventoryId ||
                    request.ExpectedRowVersion != Convert.ToBase64String(row.RowVersion))
                    return Outcome("Conflict", row.InventoryId);
                var before = InventoryPricePoint.Capture(row);
                // La importación solo refresca precios y disponibilidad: conserva la condición y el medio de pago ya registrados.
                row.UpdateImportedOffer(price, reference, discount, request.SourceUrl, request.ExternalVariantId,
                    variant.Available, DateTime.UtcNow, row.Condition, row.PaymentMethodLabel);
                InventoryPriceHistoryRecorder.Record(_context, row, before);
            }
            else
            {
                if (request.ExpectedInventoryId is not null || inspection.Outcome != "Create") return Outcome("Conflict");
                row = Inventory.CreateImportedOffer(request.ProductId, request.StoreId, price, reference, discount,
                    request.SourceUrl, request.ExternalVariantId, variant.Available, DateTime.UtcNow);
                _context.Inventories.Add(row);
                InventoryPriceHistoryRecorder.RecordBaseline(_context, row);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return Outcome("Saved", row.InventoryId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (DbUpdateException) { return Outcome("Conflict", inspection?.InventoryId); }
        catch (IrionProductUrlException exception) { return _responseService.Fail<IrionOfferConfirmationDto>(exception.Message); }
        catch (IrionRateLimitException exception) { return _responseService.Fail<IrionOfferConfirmationDto>(exception.Message); }
        catch (Exception) { return _responseService.Fail<IrionOfferConfirmationDto>("No se pudo confirmar la oferta de Irion."); }
    }

    private static bool Whole(decimal value) => value <= int.MaxValue && value >= int.MinValue && decimal.Truncate(value) == value;

    private static bool ValidVersion(string? version)
    {
        if (version is null) return false;
        Span<byte> bytes = stackalloc byte[8];
        return Convert.TryFromBase64String(version, bytes, out var length) && length == 8 &&
            Convert.ToBase64String(bytes) == version;
    }
}
