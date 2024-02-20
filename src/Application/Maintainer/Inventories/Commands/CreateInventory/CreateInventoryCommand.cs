using Domain.Entities;

namespace Application.Maintainer.Inventories.Commands.CreateInventory;

public record CreateInventoryCommand(Guid ProductId, Guid StoreId, int Price) : IRequest<Guid>;

public class CreateInventoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateInventoryCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        var inventory = Inventory.Create(request.ProductId, request.StoreId, request.Price);

        _context.Inventories.Add(inventory);

        await _context.SaveChangesAsync(cancellationToken);
        return inventory.InventoryId;
    }
}