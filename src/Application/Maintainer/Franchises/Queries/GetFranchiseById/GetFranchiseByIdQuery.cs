namespace Application.Maintainer.Franchises.Queries.GetFranchiseById;
public record GetFranchiseByIdQuery(Guid FranchiseId) : IRequest<FranchiseVM>;

public class GetFranchiseByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetFranchiseByIdQuery, FranchiseVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<FranchiseVM> Handle(GetFranchiseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Franchises
            .Where(x => x.FranchiseId == request.FranchiseId)
            .AsNoTracking()
            .ProjectTo<FranchiseVM>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);
    }
}