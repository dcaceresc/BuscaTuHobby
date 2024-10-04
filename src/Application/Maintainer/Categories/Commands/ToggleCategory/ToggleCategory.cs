namespace Application.Maintainer.Categories.Commands.ToggleCategory;

public record ToggleCategory(Guid CategoryId) : IRequest<ApiResponse>;

public class ToggleCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

            Guard.Against.NotFound(category, $"No existe categoria con la Id {request.CategoryId}");

            category.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la categoria actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la categoria");
        }
    }
}