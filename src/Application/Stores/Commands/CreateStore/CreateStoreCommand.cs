using Domain.Entities;

namespace Application.Stores.Commands.CreateStore;

public class CreateStoreCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public string address { get; set; } = default!;
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
                ranking = request.ranking,
                active = true
            };

            _context.Stores.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }

}
