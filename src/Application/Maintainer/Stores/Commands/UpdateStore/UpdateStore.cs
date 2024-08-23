namespace Application.Maintainer.Stores.Commands.UpdateStore;

public record UpdateStore : IRequest
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = default!;
    public string StoreAddress { get; init; } = default!;
    public string StoreWebSite { get; init; } = default!;

}

public class UpdateStoreHandler(IApplicationDbContext context) : IRequestHandler<UpdateStore>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateStore request, CancellationToken cancellationToken)
    {
        var store = await _context.Stores.FindAsync([request.StoreId], cancellationToken);

        Guard.Against.NotFound(request.StoreId, store);

        store.Update(request.StoreName, request.StoreAddress, request.StoreWebSite);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
