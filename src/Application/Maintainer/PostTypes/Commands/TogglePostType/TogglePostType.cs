namespace Application.Maintainer.PostTypes.Commands.TogglePostType;

public record TogglePostType(Guid PostTypeId) : IRequest<ApiResponse>;

public class TogglePostTypeHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<TogglePostType, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(TogglePostType request, CancellationToken cancellationToken)
    {
        try
        {
            var postType = await _context.PostTypes.FindAsync([request.PostTypeId], cancellationToken);

            Guard.Against.NotFound(postType, $"No existe un tipo de post con la Id {request.PostTypeId}");

            postType.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado del tipo de post actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar el tipo de post");
        }
    }
}
