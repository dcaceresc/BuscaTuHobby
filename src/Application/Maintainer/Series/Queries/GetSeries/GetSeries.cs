namespace Application.Maintainer.Series.Queries.GetSeries;

public record GetSeries : IRequest<IList<SerieDto>>;

public class GetSeriesHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetSeries, IList<SerieDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<SerieDto>> Handle(GetSeries request, CancellationToken cancellationToken)
    {
        return await _context.Series
            .AsNoTracking()
            .ProjectTo<SerieDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}