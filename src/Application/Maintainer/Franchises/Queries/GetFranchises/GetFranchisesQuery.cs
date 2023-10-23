namespace Application.Maintainer.Franchises.Queries.GetFranchises;
public class GetFranchisesQuery : IRequest<IList<FranchiseDto>>
{
}

public class GetFranchisesQueryHandler : IRequestHandler<GetFranchisesQuery, IList<FranchiseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFranchisesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<IList<FranchiseDto>> Handle(GetFranchisesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Franchises.AsNoTracking().ProjectTo<FranchiseDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }
}

