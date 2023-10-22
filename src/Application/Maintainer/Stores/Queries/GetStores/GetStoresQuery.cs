namespace Application.Maintainer.Stores.Queries.GetStores;

public class GetStoresQuery : IRequest<IList<StoreDto>>
{
    public class GetStoresQueryHandler : IRequestHandler<GetStoresQuery, IList<StoreDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;


        public GetStoresQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<StoreDto>> Handle(GetStoresQuery request, CancellationToken cancellationToken)
        {
            return await _context.Stores.AsNoTracking().ProjectTo<StoreDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}