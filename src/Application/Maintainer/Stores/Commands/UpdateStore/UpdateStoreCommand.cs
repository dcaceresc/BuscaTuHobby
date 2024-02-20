namespace Application.Maintainer.Stores.Commands.UpdateStore;

public record UpdateStoreCommand : IRequest
{
    public Guid StoreId { get; init; }
    public string StoreName { get; init; } = default!;
    public string StoreAddress { get; init; } = default!;
    public string StoreWebSite { get; init; } = default!;

}

public class UpdateStoreCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateStoreCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateStoreCommand request, CancellationToken cancellationToken)
    {
        var store = await _context.Stores.FindAsync([request.StoreId], cancellationToken);

        Guard.Against.NotFound(request.StoreId, store);

        store.Update(request.StoreName, request.StoreAddress, request.StoreWebSite);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
