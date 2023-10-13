namespace Application.Universes.Queries.GetUniverseById;

public class GetUniverseByIdQuery : IRequest<UniverseVM>
{
    public int id { get; set; }
}

public class GetUniverseByIdQueryHandler : IRequestHandler<GetUniverseByIdQuery, UniverseVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetUniverseByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UniverseVM> Handle(GetUniverseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Universes.Where(x => x.id == request.id).AsNoTracking().ProjectTo<UniverseVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}