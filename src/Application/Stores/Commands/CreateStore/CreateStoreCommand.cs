
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Stores.Commands.CreateStore;

public class CreateStoreCommand : IRequest<int>
{
    public string name { get; set; }
    public string address { get; set; }
    public int ranking { get; set; }


    public class CreateStoreCommandHandler : IRequestHandler<CreateStoreCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateStoreCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
        {
            var entity = new Store
            {
                name = request.name,
                address = request.address,
                ranking = request.ranking
            };

            _context.Stores.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }

}
