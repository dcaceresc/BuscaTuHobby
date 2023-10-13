using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Universes.Commands.UpdateUniverse;

public class UpdateUniverseCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;

    public class UpdateUniverseCommandHandler : IRequestHandler<UpdateUniverseCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUniverseCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateUniverseCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Universes.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Universe), request.id);
            }

            entity.name = request.name;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}

