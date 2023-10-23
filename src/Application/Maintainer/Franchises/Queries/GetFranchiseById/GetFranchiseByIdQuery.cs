namespace Application.Maintainer.Franchises.Queries.GetFranchiseById;
public class GetFranchiseByIdQuery : IRequest<FranchiseVM>
{
    public int id { get; set; }
}

public class GetFranchiseByIdQueryHandler : IRequestHandler<GetFranchiseByIdQuery, FranchiseVM>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFranchiseByIdQueryHandler(IApplicationDbContext context,IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<FranchiseVM> Handle(GetFranchiseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Franchises.Where(x => x.id == request.id).AsNoTracking().ProjectTo<FranchiseVM>(_mapper.ConfigurationProvider).FirstAsync(cancellationToken);
    }
}