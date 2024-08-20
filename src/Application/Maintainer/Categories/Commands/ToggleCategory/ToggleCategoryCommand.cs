namespace Application.Maintainer.Categories.Commands.ToggleCategory;
public record ToggleCategoryCommand(Guid CategoryId) : IRequest<ApiResponse>;

public class ToggleSubCategoryCommandHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleCategoryCommand, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleCategoryCommand request, CancellationToken cancellationToken)
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
        catch (Exception ex)
        {
            return _responseService.Fail(ex.Message);
        }
    }
}