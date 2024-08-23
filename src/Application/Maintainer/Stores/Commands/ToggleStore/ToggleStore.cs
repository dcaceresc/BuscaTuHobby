namespace Application.Maintainer.Stores.Commands.ToggleStore;

public record ToggleStore(Guid SerieId) : IRequest;

public class ToggleStoreHandler(IApplicationDbContext context) : IRequestHandler<ToggleStore>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleStore request, CancellationToken cancellationToken)
    {
        var serie = await _context.Stores.FindAsync([request.SerieId], cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        serie.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}