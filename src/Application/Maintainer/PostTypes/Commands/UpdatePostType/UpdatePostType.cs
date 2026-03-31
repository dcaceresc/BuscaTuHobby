namespace Application.Maintainer.PostTypes.Commands.UpdatePostType;

public record UpdatePostType(Guid PostTypeId, string PostTypeName) : IRequest<ApiResponse>;

public class UpdatePostTypeHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdatePostType, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdatePostType request, CancellationToken cancellationToken)
    {
        try
        {
            var postType = await _context.PostTypes.FindAsync([request.PostTypeId], cancellationToken);

            Guard.Against.NotFound(postType, $"No existe un tipo de post con la Id {request.PostTypeId}");

            postType.Update(request.PostTypeName);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Tipo de post actualizado correctamente");
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
