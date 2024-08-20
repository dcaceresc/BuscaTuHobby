namespace Application.Maintainer.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string CategoryName, Guid GroupId) : IRequest<ApiResponse>;

public class CreateSubCategoryCommandHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateCategoryCommand, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = Category.Create(request.CategoryName, request.GroupId);

            _context.Categories.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Categoría creada exitosamente");
        }
        catch (Exception ex)
        {
            return _responseService.Fail(ex.Message);
        }        
    }
}
