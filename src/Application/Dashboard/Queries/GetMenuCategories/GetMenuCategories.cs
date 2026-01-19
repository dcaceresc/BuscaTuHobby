namespace Application.Dashboard.Queries.GetMenuCategories;
                
public record GetMenuCategories() : IRequest<ApiResponse<List<MenuCategoryDto>>>;

public class GetMenuCategoriesHandler(IApplicationDbContext context, IApiResponseService responseService) 
    : IRequestHandler<GetMenuCategories, ApiResponse<List<MenuCategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<MenuCategoryDto>>> Handle(GetMenuCategories request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryOrder)
                .Select(c => new MenuCategoryDto
                {
                    CategoryName = c.CategoryName,
                    CategoryIcon = c.CategoryIcon,
                    CategorySlug = c.CategorySlug
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(categories);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<MenuCategoryDto>>("Ah ocurrido un error al obtener las categorías del menú.");
        }
    }
}