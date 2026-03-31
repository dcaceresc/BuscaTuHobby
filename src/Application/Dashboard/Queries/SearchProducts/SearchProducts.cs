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
                    StoreCount = p.Inventories.Count(i => i.IsActive),
                    MinPrice = p.Inventories.Where(i => i.IsActive).Any()
                        ? p.Inventories.Where(i => i.IsActive).Min(i => i.Price)
                        : 0,
                    MaxPrice = p.Inventories.Where(i => i.IsActive).Any()
                        ? p.Inventories.Where(i => i.IsActive).Max(i => i.Price)
                        : 0,
                    BestDiscount = p.Inventories.Where(i => i.IsActive && i.DiscountPercentage > 0).Any()
                        ? p.Inventories.Where(i => i.IsActive && i.DiscountPercentage > 0).Max(i => i.DiscountPercentage)
                        : 0,
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
