using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Stores.Commands.ToggleStore;

public class ToggleStoreCommand : IRequest
{
    public int id { get; set; }

    public class ToggleStoreCommandHandler : IRequestHandler<ToggleStoreCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleStoreCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleStoreCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Stores.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Store), request.id);

            entity.active = !entity.active;
           

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}