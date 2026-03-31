namespace Application.Maintainer.Posts.Queries.GetPosts;

public record GetPosts : IRequest<ApiResponse<List<PostDto>>>;

public class GetPostsHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetPosts, ApiResponse<List<PostDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<PostDto>>> Handle(GetPosts request, CancellationToken cancellationToken)
    {
        try
        {
            var posts = await _context.Posts
                .Include(x => x.PostType)
                .Include(x => x.PostCategories)
                .AsNoTracking()
                .Select(x => new PostDto
                {
                    PostId = x.PostId,
                    PostTitle = x.PostTitle,
                    PostTypeName = x.PostType.PostTypeName,
                    Categories = x.PostCategories.Select(pc => pc.Category.CategoryName).ToList(),
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(posts);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<PostDto>>("Ocurrió un error al obtener los posts");
        }
    }
}
