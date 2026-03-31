namespace Application.Maintainer.Posts.Queries.GetPostById;

public record GetPostById(Guid PostId) : IRequest<ApiResponse<PostVM>>;

public class GetPostByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetPostById, ApiResponse<PostVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<PostVM>> Handle(GetPostById request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _context.Posts
                .Include(x => x.PostCategories)
                .AsNoTracking()
                .Select(x => new PostVM
                {
                    PostId = x.PostId,
                    PostTitle = x.PostTitle,
                    PostContent = x.PostContent,
                    PostTypeId = x.PostTypeId,
                    CategoryIds = x.PostCategories.Select(pc => pc.CategoryId).ToList()
                })
                .FirstOrDefaultAsync(x => x.PostId == request.PostId, cancellationToken);

            Guard.Against.NotFound(post, $"No existe un post con la Id {request.PostId}");

            return _responseService.Success(post);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<PostVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<PostVM>("Ocurrió un error al obtener el post");
        }
    }
}
