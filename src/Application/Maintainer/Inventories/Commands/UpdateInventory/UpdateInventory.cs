namespace Application.Maintainer.Inventories.Commands.UpdateInventory;

public record UpdateInventory : IRequest<ApiResponse>
{
    public Guid InventoryId { get; init; }
    public Guid ProductId { get; init; }
    public Guid StoreId { get; init; }
    public int Price { get; init; }
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

            inventory.Update(request.ProductId, request.StoreId, request.Price);

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