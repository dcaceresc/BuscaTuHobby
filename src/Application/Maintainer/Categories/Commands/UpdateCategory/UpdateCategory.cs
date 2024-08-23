namespace Application.Maintainer.Categories.Commands.UpdateCategory;

public record UpdateCategory(Guid CategoryId, string CategoryName, Guid GroupId) : IRequest<ApiResponse>;

public class UpdateSubCategoryHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe categoria con la Id {request.CategoryId}");

            entity.Update(request.CategoryName, request.GroupId);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Categoría actualizada exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar la categoría");
        }
    }
}
