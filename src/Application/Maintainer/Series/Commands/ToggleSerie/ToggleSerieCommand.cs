namespace Application.Maintainer.Series.Commands.ToggleSerie;

public record ToggleSerieCommand(Guid SerieId) : IRequest;
public class ToggleSerieCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleSerieCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleSerieCommand request, CancellationToken cancellationToken)
    {
        var serie = await _context.Series.FindAsync([request.SerieId], cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        serie.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);
    }
}