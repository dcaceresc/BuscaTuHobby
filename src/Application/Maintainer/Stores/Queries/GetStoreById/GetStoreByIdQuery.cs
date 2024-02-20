namespace Application.Maintainer.Stores.Queries.GetStoreById;

public record GetStoreByIdQuery(Guid StoreId) : IRequest<StoreVM>;

public class GetStoreByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetStoreByIdQuery, StoreVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<StoreVM> Handle(GetStoreByIdQuery request, CancellationToken cancellationToken)
    {
        var store = await _context.Stores
               .AsNoTracking()
               .ProjectTo<StoreVM>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync(x => x.StoreId == request.StoreId, cancellationToken);

        Guard.Against.NotFound(request.StoreId, store);

        return store;
    }
}
