namespace Application.Maintainer.Series.Queries.GetSeries;

public class GetSeriesQuery : IRequest<IList<SerieDto>>
{
    public class GetSeriesQueryHandler : IRequestHandler<GetSeriesQuery, IList<SerieDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSeriesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<SerieDto>> Handle(GetSeriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Series.AsNoTracking().ProjectTo<SerieDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}

