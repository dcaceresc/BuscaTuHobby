namespace Application.Maintainer.Inventories.Commands.ToggleInventory;

public record ToggleInventoryCommand(Guid InventoryId) : IRequest;
public class ToggleSaleCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleInventoryCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories.FindAsync([request.InventoryId], cancellationToken);

        Guard.Against.NotFound(request.InventoryId, inventory);

        inventory.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}