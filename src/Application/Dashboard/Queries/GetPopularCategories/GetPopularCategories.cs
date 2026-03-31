namespace Application.Dashboard.Queries.GetPopularCategories;

public record GetPopularCategories() : IRequest<ApiResponse<List<PopularCategoryDto>>>;

public class GetPopularCategoriesHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetPopularCategories, ApiResponse<List<PopularCategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<PopularCategoryDto>>> Handle(GetPopularCategories request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.CategoryName,
                    ProductCount = c.ProductCategories.Count(pc => pc.Product.IsActive)
                })
                .Where(c => c.ProductCount > 0)
                .OrderByDescending(c => c.ProductCount)
                .Take(4)
                .ToListAsync(cancellationToken);

            var totalProducts = categories.Sum(c => c.ProductCount);

            var result = categories.Select(c => new PopularCategoryDto
            {
                CategoryName = c.CategoryName,
                ProductCount = c.ProductCount,
                Percentage = totalProducts > 0
                    ? (int)Math.Round((double)c.ProductCount / totalProducts * 100)
                    : 0
            }).ToList();

            return _responseService.Success(result);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<PopularCategoryDto>>("Ha ocurrido un error al obtener las categorías populares.");
        }
    }
}
