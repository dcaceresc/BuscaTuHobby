namespace Application.Maintainer.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<IList<ProductDto>>;

public class GetProductsQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetProductsQuery, IList<ProductDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<IList<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Products.
            Include(x => x.Scale).
            Include(x => x.Manufacturer).
            Include(x => x.ProductCategories).
            Include(x => x.Franchise).
            ThenInclude(x => x.Series).
            AsNoTracking().
            ProjectTo<ProductDto>(_mapper.ConfigurationProvider).
            ToListAsync(cancellationToken);

    }
}