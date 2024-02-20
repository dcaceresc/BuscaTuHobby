namespace Application.Maintainer.Inventories.Commands.UpdateInventory;

public record UpdateInventoryCommand : IRequest
{
    public Guid InventoryId { get; init; }
    public Guid ProductId { get; init; }
    public Guid StoreId { get; init; }
    public int Price { get; init; }
}

public class UpdateInventoryCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateInventoryCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = await _context.Inventories.FindAsync([request.InventoryId], cancellationToken);

        Guard.Against.NotFound(request.InventoryId, inventory);

        inventory.Update(request.ProductId, request.StoreId, request.Price);

        await _context.SaveChangesAsync(cancellationToken);
    }
}