namespace Application.Dashboard.Commands.IncrementPostViewCount;

public record IncrementPostViewCount(Guid PostId) : IRequest<ApiResponse>;

public class IncrementPostViewCountHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<IncrementPostViewCount, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(IncrementPostViewCount request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _context.Posts.FindAsync([request.PostId], cancellationToken);

            Guard.Against.NotFound(post, $"No existe un post con la Id {request.PostId}");

            post.IncrementViewCount();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Vista registrada correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrio un error al registrar la vista del post");
        }
    }
}
