namespace Application.Maintainer.Series.Commands.CreateSerie;

public record CreateSerieCommand(string SerieName, Guid FranchiseId) : IRequest<Guid>;

public class CreateSerieCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateSerieCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateSerieCommand request, CancellationToken cancellationToken)
    {
        var serie = Serie.Create(request.SerieName, request.FranchiseId);

        _context.Series.Add(serie);

        await _context.SaveChangesAsync(cancellationToken);

        return serie.SerieId;
    }
}