using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Universes.Commands.CreateUniverse;

public class CreateUniverseCommand : IRequest<int>
{
    public string name { get; set; }

    public class CreateUniverseCommandHandler : IRequestHandler<CreateUniverseCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateUniverseCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateUniverseCommand request, CancellationToken cancellationToken)
        {
            var entity = new Universe
            {
                name = request.name
            };

            _context.Universes.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }
}

    

