namespace Application.Maintainer.Stores.Queries.GetStores;

public record GetStoresQuery : IRequest<IList<StoreDto>>;

public class GetStoresQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetStoresQuery, IList<StoreDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
    {
        return await _context.Stores
            .AsNoTracking()
            .ProjectTo<StoreDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}