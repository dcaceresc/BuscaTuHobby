namespace Application.Maintainer.Series.Queries.GetSeriesByFranchise;
public class GetSeriesByFranchiseQuery : IRequest<IList<SerieByFranchiseDto>>
{
    public int franchiseId { get; set; }
}

public class GetSeriesByFranchiseQueryHandler : IRequestHandler<GetSeriesByFranchiseQuery, IList<SerieByFranchiseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSeriesByFranchiseQueryHandler(IApplicationDbContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<IList<SerieByFranchiseDto>> Handle(GetSeriesByFranchiseQuery request, CancellationToken cancellationToken)
    {
        return await _context.Series.Where(x => x.franchiseId == request.franchiseId).AsNoTracking().ProjectTo<SerieByFranchiseDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
} 
