namespace Application.Maintainer.Series.Commands.ToggleSerie;

public record ToggleSerie(Guid SerieId) : IRequest;
public class ToggleSerieHandler(IApplicationDbContext context) : IRequestHandler<ToggleSerie>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleSerie request, CancellationToken cancellationToken)
    {
        var serie = await _context.Series.FindAsync([request.SerieId], cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        serie.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);
    }
}