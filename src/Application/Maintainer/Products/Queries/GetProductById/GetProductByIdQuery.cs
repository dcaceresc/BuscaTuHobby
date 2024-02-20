namespace Application.Maintainer.Products.Queries.GetProductById;
public record GetProductByIdQuery(Guid ProductId) : IRequest<ProductVM>;

public class GetProductByIdQueryHandler(IApplicationDbContext context, IMapper mapper) : IRequestHandler<GetProductByIdQuery, ProductVM>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<ProductVM> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
               .Include(x => x.Scale)
               .Include(x => x.Manufacturer)
               .Include(x => x.ProductCategories)
               .Include(x => x.Franchise)
               .ThenInclude(x => x.Series)
               .AsNoTracking()
               .ProjectTo<ProductVM>(_mapper.ConfigurationProvider)
               .FirstOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);

        Guard.Against.NotFound(request.ProductId, product);

        return product;
    }
}
