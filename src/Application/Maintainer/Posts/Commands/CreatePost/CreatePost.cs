namespace Application.Maintainer.Posts.Commands.CreatePost;

public record CreatePost : IRequest<ApiResponse<Guid>>
{
    public string PostTitle { get; init; } = default!;
    public string PostContent { get; init; } = default!;
    public Guid PostTypeId { get; init; }
    public IList<Guid> CategoryIds { get; init; } = default!;
}

public class CreatePostHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreatePost, ApiResponse<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<Guid>> Handle(CreatePost request, CancellationToken cancellationToken)
    {
        try
        {
            var post = Post.Create(request.PostTitle, request.PostContent, request.PostTypeId);

            _context.Posts.Add(post);

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var categoryId in request.CategoryIds)
            {
                var postCategory = post.AssignCategory(categoryId);
                _context.PostCategories.Add(postCategory);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success(post.PostId);
        }
        catch (Exception)
        {
            return _responseService.Fail<Guid>("No se pudo crear el post");
        }
    }
}
