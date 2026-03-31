namespace Application.Maintainer.Posts.Commands.TogglePost;

public record TogglePost(Guid PostId) : IRequest<ApiResponse>;

public class TogglePostHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<TogglePost, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(TogglePost request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _context.Posts.FindAsync([request.PostId], cancellationToken);

            Guard.Against.NotFound(post, $"No existe un post con la Id {request.PostId}");

            post.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado del post actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al actualizar el post");
        }
    }
}
