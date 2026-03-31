namespace Application.Maintainer.PostTypes.Commands.CreatePostType;

public record CreatePostType(string PostTypeName) : IRequest<ApiResponse>;

public class CreatePostTypeHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreatePostType, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreatePostType request, CancellationToken cancellationToken)
    {
        try
        {
            var postType = PostType.Create(request.PostTypeName);

            _context.PostTypes.Add(postType);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Tipo de post creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Ocurrió un error al crear el tipo de post");
        }
    }
}
