namespace Application.Maintainer.Franchises.Queries.GetFranchises;
public record GetFranchisesQuery : IRequest<IList<FranchiseDto>>;

public class GetFranchisesQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFranchisesQuery, IList<FranchiseDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<FranchiseDto>> Handle(GetFranchisesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Franchises
            .AsNoTracking()
            .ProjectTo<FranchiseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}

