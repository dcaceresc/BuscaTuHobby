using Domain.Entities;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public class CreateStoreCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public string address { get; set; } = default!;
    public string webSite { get; set; } = default!;


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
                webSite = request.webSite,
                active = true
            };

            _context.Stores.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }

}
