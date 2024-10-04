namespace Application.Maintainer.Categories.Commands.UpdateCategory;

public record UpdateCategory(Guid CategoryId, string CategoryName) : IRequest<ApiResponse>;

public class UpdateScaleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateCategory, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateCategory request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

            Guard.Against.NotFound(category, $"No existe una categoria con la Id {request.CategoryId}");

            category.Update(request.CategoryName);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Escala actualizada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar la escala");
        }
    }
}