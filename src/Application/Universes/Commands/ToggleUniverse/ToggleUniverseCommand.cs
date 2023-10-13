using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Universes.Commands.ToggleUniverse;

public class ToggleUniverseCommand : IRequest
{
    public int id { get; set; }

    public class ToggleUniverseCommandHandler : IRequestHandler<ToggleUniverseCommand>
    {
        private readonly IApplicationDbContext _context;
        public ToggleUniverseCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleUniverseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Universes.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Universe), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}


