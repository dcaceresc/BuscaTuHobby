using Application.Common.PriceHistory;

namespace Application.Maintainer.Inventories.Commands.UpdateInventory;

public record UpdateInventory : IRequest<ApiResponse>
{
    public Guid InventoryId { get; init; }
    public Guid ProductId { get; init; }
    public Guid StoreId { get; init; }
    public int Price { get; init; }
    public int OriginalPrice { get; init; }
    public int DiscountPercentage { get; init; }

    // Condición como entero (0 nueva, 1 refabricada, 2 usada); el valor inicial mantiene compilando a los llamadores previos.
    public int Condition { get; init; } = (int)ProductCondition.New;
    public string? PaymentMethodLabel { get; init; }
}

public class UpdateInventoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateInventory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateInventory request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _context.Inventories.FindAsync([request.InventoryId], cancellationToken);

            Guard.Against.NotFound(inventory, $"No existe inventario con Id {request.InventoryId}");

            if (!OfferConditionRules.IsValidCondition(request.Condition))
                return _responseService.Fail(OfferConditionRules.InvalidConditionMessage);

            if (!OfferConditionRules.IsValidPaymentMethodLabel(request.PaymentMethodLabel))
                return _responseService.Fail(OfferConditionRules.InvalidPaymentMethodLabelMessage);

            if (await _context.Inventories.AnyAsync(i => i.InventoryId != request.InventoryId &&
                i.StoreId == request.StoreId && i.ProductId == request.ProductId, cancellationToken))
                return _responseService.Fail("No se pudo actualizar el inventario");

            var before = InventoryPricePoint.Capture(inventory);
            inventory.Update(request.ProductId, request.StoreId, request.Price, request.OriginalPrice,
                request.DiscountPercentage, (ProductCondition)request.Condition, request.PaymentMethodLabel);
            InventoryPriceHistoryRecorder.Record(_context, inventory, before);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Inventario actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el inventario");
        }


    }
}