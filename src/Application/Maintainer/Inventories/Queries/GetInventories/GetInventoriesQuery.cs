namespace Application.Maintainer.Inventories.Queries.GetInventories;

public class GetInventoriesQuery : IRequest<IList<InventoryDto>>
{
    public class GetSalesQueryHandler : IRequestHandler<GetInventoriesQuery, IList<InventoryDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSalesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<InventoryDto>> Handle(GetInventoriesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Inventories.AsNoTracking().ProjectTo<InventoryDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
        }
    }
}