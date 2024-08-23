namespace Application.Maintainer.Categories.Commands.CreateCategory;

public record CreateCategory(string CategoryName, Guid GroupId) : IRequest<ApiResponse>;

public class CreateSubCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Category.Create(request.CategoryName, request.GroupId);

            _context.Categories.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Categoría creada exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear la categoría");
        }
    }
}
