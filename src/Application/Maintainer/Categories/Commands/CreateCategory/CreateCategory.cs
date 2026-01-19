namespace Application.Maintainer.Categories.Commands.CreateCategory;

public record CreateCategory(string CategoryName, string CategoryIcon, int CategoryOrder, string CategorySlug) : IRequest<ApiResponse>;

public class CreateCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var category = Category.Create(request.CategoryName, request.CategoryIcon, request.CategoryOrder, request.CategorySlug);

            _context.Categories.Add(category);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Escala creada correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al crear la escala");
        }
    }
}