namespace Application.Maintainer.Inventories.Commands.ToggleInventory;

public record ToggleInventory(Guid InventoryId) : IRequest<ApiResponse>;
public class ToggleInventoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleInventory,ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleInventory request, CancellationToken cancellationToken)
    {
        try
        {
            var inventory = await _context.Inventories.FindAsync([request.InventoryId], cancellationToken);

            Guard.Against.NotFound(inventory, $"No existe el inventario con la Id {request.InventoryId}");

            inventory.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Inventario actualizado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el inventario");
        }
    }
}