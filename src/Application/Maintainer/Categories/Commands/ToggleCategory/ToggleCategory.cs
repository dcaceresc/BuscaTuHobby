namespace Application.Maintainer.Categories.Commands.ToggleCategory;
public record ToggleCategory(Guid CategoryId) : IRequest<ApiResponse>;

public class ToggleSubCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe categoria con la Id {request.CategoryId}");

            entity.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la categoria ha sido actualizado exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el estado de la categoria");
        }
    }
}