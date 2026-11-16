using Application.Common.PriceHistory;

namespace Application.Maintainer.Inventories.Commands.CreateInventory;

// La condición viaja como entero (0 nueva, 1 refabricada, 2 usada) y es opcional para los llamadores existentes.
public record CreateInventory(Guid ProductId, Guid StoreId, int Price, int OriginalPrice, int DiscountPercentage,
    int Condition = (int)ProductCondition.New, string? PaymentMethodLabel = null) : IRequest<ApiResponse>;

public class CreateInventoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateInventory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateInventory request, CancellationToken cancellationToken)
    {
        try
        {
            if (!OfferConditionRules.IsValidCondition(request.Condition))
                return _responseService.Fail(OfferConditionRules.InvalidConditionMessage);

            if (!OfferConditionRules.IsValidPaymentMethodLabel(request.PaymentMethodLabel))
                return _responseService.Fail(OfferConditionRules.InvalidPaymentMethodLabelMessage);

            if (await _context.Inventories.AnyAsync(i => i.StoreId == request.StoreId && i.ProductId == request.ProductId, cancellationToken))
                return _responseService.Fail("No se pudo crear el inventario");

            var inventory = Inventory.Create(request.ProductId, request.StoreId, request.Price, request.OriginalPrice,
                request.DiscountPercentage, (ProductCondition)request.Condition, request.PaymentMethodLabel);

            _context.Inventories.Add(inventory);
            InventoryPriceHistoryRecorder.RecordBaseline(_context, inventory);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Inventario creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el inventario");
        }
    }
}
