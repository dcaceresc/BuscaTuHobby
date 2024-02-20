namespace Application.Maintainer.Stores.Commands.ToggleStore;

public record ToggleStoreCommand(Guid SerieId) : IRequest;

public class ToggleStoreCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleStoreCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleStoreCommand request, CancellationToken cancellationToken)
    {
        var serie = await _context.Stores.FindAsync([request.SerieId], cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        serie.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}