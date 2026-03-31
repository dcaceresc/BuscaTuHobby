namespace Application.Dashboard.Queries.GetMostSearchedProducts;

public record GetMostSearchedProducts() : IRequest<ApiResponse<List<MostSearchedProductDto>>>;

public class GetMostSearchedProductsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetMostSearchedProducts, ApiResponse<List<MostSearchedProductDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<MostSearchedProductDto>>> Handle(GetMostSearchedProducts request, CancellationToken cancellationToken)
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.ProductViewCount)
                .Take(4)
                .Select(p => new MostSearchedProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ProductViewCount = p.ProductViewCount,
                    StoreCount = p.Inventories.Count(i => i.IsActive),
                    StoreNames = p.Inventories
                        .Where(i => i.IsActive)
                        .Select(i => i.Store.StoreName)
                        .Distinct()
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(products);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<MostSearchedProductDto>>("Ha ocurrido un error al obtener los productos mas buscados.");
        }
    }
}
