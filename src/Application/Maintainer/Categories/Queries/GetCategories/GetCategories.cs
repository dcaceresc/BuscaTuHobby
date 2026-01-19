namespace Application.Maintainer.Categories.Queries.GetCategories;

public record GetCategories : IRequest<ApiResponse<List<CategoryDto>>>;

public class GetCategoriesHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetCategories, ApiResponse<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategories request, CancellationToken cancellationToken)
    {
        try
        {
            var scales = await _context.Categories
                .AsNoTracking()
                .Select(x => new CategoryDto
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    CategoryIcon = x.CategoryIcon,
                    CategoryOrder = x.CategoryOrder,
                    CategorySlug = x.CategorySlug,
                    IsActive = x.IsActive,
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(scales);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<CategoryDto>>("Ocurrió un error al obtener las categorias");
        }
    }
}

