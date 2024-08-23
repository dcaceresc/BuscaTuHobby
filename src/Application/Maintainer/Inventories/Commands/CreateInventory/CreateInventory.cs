namespace Application.Maintainer.Inventories.Commands.CreateInventory;

public record CreateInventory(Guid ProductId, Guid StoreId, int Price) : IRequest<ApiResponse>;

public class CreateInventoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateInventory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateInventory request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = Inventory.Create(request.ProductId, request.StoreId, request.Price);

            _context.Inventories.Add(inventory);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Inventario creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el inventario");
        }
    }
}