namespace Application.Dashboard.Queries.GetRecentPosts;

public record GetRecentPosts() : IRequest<ApiResponse<List<RecentPostDto>>>;

public class GetRecentPostsHandler(IApplicationDbContext context, IApiResponseService responseService)
    : IRequestHandler<GetRecentPosts, ApiResponse<List<RecentPostDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<RecentPostDto>>> Handle(GetRecentPosts request, CancellationToken cancellationToken)
    {
        try
        {
            var posts = await _context.Posts
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Created)
                .Take(4)
                .Select(p => new RecentPostDto
                {
                    PostId = p.PostId,
                    PostTitle = p.PostTitle,
                    PostContent = p.PostContent,
                    PostTypeName = p.PostType.PostTypeName,
                    PostViewCount = p.PostViewCount
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(posts);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<RecentPostDto>>("Ha ocurrido un error al obtener los posts recientes.");
        }
    }
}
