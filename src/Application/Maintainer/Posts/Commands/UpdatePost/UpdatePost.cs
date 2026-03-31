namespace Application.Maintainer.Posts.Commands.UpdatePost;

public record UpdatePost : IRequest<ApiResponse>
{
    public Guid PostId { get; init; }
    public string PostTitle { get; init; } = default!;
    public string PostContent { get; init; } = default!;
    public Guid PostTypeId { get; init; }
    public IList<Guid> CategoryIds { get; init; } = default!;
}

public class UpdatePostHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdatePost, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdatePost request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _context.Posts.FindAsync([request.PostId], cancellationToken);

            Guard.Against.NotFound(post, $"No existe un post con la Id {request.PostId}");

            post.Update(request.PostTitle, request.PostContent, request.PostTypeId);

            // Eliminar categorías existentes y reasignar
            var existingCategories = await _context.PostCategories
                .Where(pc => pc.PostId == request.PostId)
                .ToListAsync(cancellationToken);

            _context.PostCategories.RemoveRange(existingCategories);

            foreach (var categoryId in request.CategoryIds)
            {
                var postCategory = post.AssignCategory(categoryId);
                _context.PostCategories.Add(postCategory);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Post actualizado correctamente");
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
