using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Inventories.Commands.ToggleInventory;

public class ToggleInventoryCommand : IRequest
{
    public int id { get; set; }

    public class ToggleSaleCommandHandler : IRequestHandler<ToggleInventoryCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleSaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleInventoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Inventories.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Inventory), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
