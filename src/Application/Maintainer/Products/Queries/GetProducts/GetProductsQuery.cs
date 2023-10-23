namespace Application.Maintainer.Products.Queries.GetProducts;

public class GetProductsQuery : IRequest<IList<ProductDto>>
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IList<ProductDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            return await _context.Products.
                Include(x => x.Scale).
                Include(x => x.Manufacturer).
                Include(x => x.CategoryProducts).
                Include(x => x.Franchise).
                ThenInclude(x => x.Serie).
                AsNoTracking().
                ProjectTo<ProductDto>(_mapper.ConfigurationProvider).
                ToListAsync(cancellationToken);

        }
    }
}