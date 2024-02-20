namespace Application.Maintainer.Series.Queries.GetSeriesByFranchise;
public record GetSeriesByFranchiseQuery(Guid FranchiseId) : IRequest<IList<SerieByFranchiseDto>>;

public class GetSeriesByFranchiseQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetSeriesByFranchiseQuery, IList<SerieByFranchiseDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<SerieByFranchiseDto>> Handle(GetSeriesByFranchiseQuery request, CancellationToken cancellationToken)
    {
        return await _context.Series
            .Where(x => x.FranchiseId == request.FranchiseId)
            .AsNoTracking()
            .ProjectTo<SerieByFranchiseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
