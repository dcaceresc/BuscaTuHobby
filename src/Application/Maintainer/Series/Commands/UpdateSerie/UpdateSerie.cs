namespace Application.Maintainer.Series.Commands.UpdateSerie;

public record UpdateSerie(Guid SerieId, string SerieName, Guid FranchiseId) : IRequest;
public class UpdateSerieHandler(IApplicationDbContext context) : IRequestHandler<UpdateSerie>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateSerie request, CancellationToken cancellationToken)
    {
        var serie = await _context.Series.FindAsync([request.SerieId], cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        serie.Update(request.SerieName, request.FranchiseId);

        await _context.SaveChangesAsync(cancellationToken);

    }
}