namespace Application.Maintainer.Series.Commands.CreateSerie;

public record CreateSerie(string SerieName, Guid FranchiseId) : IRequest<Guid>;

public class CreateSerieHandler(IApplicationDbContext context) : IRequestHandler<CreateSerie, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateSerie request, CancellationToken cancellationToken)
    {
        var serie = Serie.Create(request.SerieName, request.FranchiseId);

        _context.Series.Add(serie);

        await _context.SaveChangesAsync(cancellationToken);

        return serie.SerieId;
    }
}