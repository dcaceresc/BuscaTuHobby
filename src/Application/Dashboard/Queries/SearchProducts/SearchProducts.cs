namespace Application.Dashboard.Queries.SearchProducts;

public record SearchProducts(string Term) : IRequest<ApiResponse<List<SearchProductDto>>>;

public class SearchProductsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<SearchProducts, ApiResponse<List<SearchProductDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<SearchProductDto>>> Handle(SearchProducts request, CancellationToken cancellationToken)
    {
        try
        {
            var term = request.Term?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(term))
            {
                return _responseService.Success(new List<SearchProductDto>());
            }

            var products = await _context.Products
                .Where(p => p.IsActive &&
                    (p.ProductName.ToLower().Contains(term) ||
                     p.ProductDescription.ToLower().Contains(term) ||
                     p.Manufacturer.ManufacturerName.ToLower().Contains(term) ||
                     p.Franchise.FranchiseName.ToLower().Contains(term) ||
                     p.ProductCategories.Any(pc => pc.Category.CategoryName.ToLower().Contains(term))))
                .OrderByDescending(p => p.ProductViewCount)
                .Take(10)
                .Select(p => new SearchProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    ManufacturerName = p.Manufacturer.ManufacturerName,
                    FranchiseName = p.Franchise.FranchiseName,
                    ProductViewCount = p.ProductViewCount,
                    StoreCount = p.Inventories
                        .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                        .Select(i => i.StoreId).Distinct().Count(),
                    MinPrice = p.Inventories
                        .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                        .Select(i => (int?)i.Price).Min() ?? 0,
                    MaxPrice = p.Inventories
                        .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false)
                        .Select(i => (int?)i.Price).Max() ?? 0,
                    BestDiscount = p.Inventories
                        .Where(i => i.IsActive && i.Store.IsActive && i.Product.IsActive && i.Availability != false &&
                            i.DiscountPercentage > 0)
                        .Select(i => (int?)i.DiscountPercentage).Max() ?? 0,
                    Categories = p.ProductCategories
                        .Where(pc => pc.Category.IsActive)
                        .Select(pc => pc.Category.CategoryName)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(products);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<SearchProductDto>>("Ha ocurrido un error al buscar productos.");
        }
    }
}
