using Domain.Entities;

namespace Application.Inventories.Commands.CreateInventory;

public class CreateInventoryCommand : IRequest<int>
{
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateInventoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
        {
            var entity = new Inventory
            {
                gunplaId = request.gunplaId,
                storeId = request.storeId,
                price = request.price
            };

            _context.Inventories.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }

}
