namespace Application.Maintainer.Series.Queries.GetSeries;

public record GetSeriesQuery : IRequest<IList<SerieDto>>;

public class GetSeriesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetSeriesQuery, IList<SerieDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<SerieDto>> Handle(GetSeriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Series
            .AsNoTracking()
            .ProjectTo<SerieDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}