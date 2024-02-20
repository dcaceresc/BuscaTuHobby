using Domain.Entities;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public record CreateStoreCommand(string StoreName, string StoreAddress, string StoreWebSite) : IRequest<Guid>;

public class CreateStoreCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateStoreCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = Store.Create(request.StoreName, request.StoreAddress, request.StoreWebSite);

        _context.Stores.Add(store);

        await _context.SaveChangesAsync(cancellationToken);

        return store.StoreId;
    }
}