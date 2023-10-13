using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Inventories.Commands.UpdateInventory;

public class UpdateInventoryCommand : IRequest
{
    public int id { get; set; }
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateInventoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateInventoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Inventories.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Inventory), request.id);

            entity.gunplaId = request.gunplaId;
            entity.storeId = request.storeId;
            entity.price = request.price;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}