namespace Application.Maintainer.Stores.Commands.CreateStore;

public record CreateStore(string StoreName, string StoreAddress, string StoreWebSite) : IRequest<Guid>;

public class CreateStoreHandler(IApplicationDbContext context) : IRequestHandler<CreateStore, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateStore request, CancellationToken cancellationToken)
    {
        var store = Store.Create(request.StoreName, request.StoreAddress, request.StoreWebSite);

        _context.Stores.Add(store);

        await _context.SaveChangesAsync(cancellationToken);

        return store.StoreId;
    }
}