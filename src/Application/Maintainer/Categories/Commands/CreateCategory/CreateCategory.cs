namespace Application.Maintainer.Categories.Commands.CreateCategory;

public record CreateCategory(string CategoryName) : IRequest<ApiResponse>;

public class CreateCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var category = Category.Create(request.CategoryName);

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