namespace Application.Maintainer.Series.Queries.GetSerieById;

public record GetSerieById(Guid SerieId) : IRequest<SerieVM>;
public class GetSerieByIdHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetSerieById, SerieVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<SerieVM> Handle(GetSerieById request, CancellationToken cancellationToken)
    {
        var serie = await _context.Series
            .AsNoTracking()
            .ProjectTo<SerieVM>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.SerieId == request.SerieId, cancellationToken);

        Guard.Against.NotFound(request.SerieId, serie);

        return serie;
    }
}