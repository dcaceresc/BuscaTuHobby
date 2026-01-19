namespace Application.Maintainer.Categories.Queries.GetCategoryById;

public record GetCategoryById(Guid CategoryId) : IRequest<ApiResponse<CategoryVM>>;

public class GetCategoryByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetCategoryById, ApiResponse<CategoryVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<CategoryVM>> Handle(GetCategoryById request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _context.Categories
              .AsNoTracking()
              .Select(x => new CategoryVM
              {
                  CategoryId = x.CategoryId,
                  CategoryName = x.CategoryName,
                  CategoryIcon = x.CategoryIcon,
                  CategoryOrder = x.CategoryOrder,
                  CategorySlug  = x.CategorySlug
              })
              .FirstOrDefaultAsync(x => x.CategoryId == request.CategoryId, cancellationToken);

            Guard.Against.NotFound(category, $"No existe una categoria con la Id {request.CategoryId}");

            return _responseService.Success(category);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<CategoryVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<CategoryVM>("Ocurrió un error al obtener la categoria");
        }
    }
}