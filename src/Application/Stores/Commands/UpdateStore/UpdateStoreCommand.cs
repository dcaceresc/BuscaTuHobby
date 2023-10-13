using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Stores.Commands.UpdateStore;

public class UpdateStoreCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string address { get; set; } = default!;
    public int ranking { get; set; }

    public class UpdateStoreCommandHandler : IRequestHandler<UpdateStoreCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateStoreCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Stores.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Store), request.id);
            }

            entity.name = request.name;
            entity.address = request.address;
            entity.ranking = request.ranking;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
