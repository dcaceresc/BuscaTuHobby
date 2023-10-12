namespace Application.Series.Queries.GetSerieById;

public class GetSerieByIdQuery : IRequest<SerieVM>
{
    public int id { get; set; }
}

public class GetSerieByIdQueryHandler : IRequestHandler<GetSerieByIdQuery, SerieVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSerieByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SerieVM> Handle(GetSerieByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Series.Where(x => x.id == request.id).AsNoTracking().ProjectTo<SerieVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}