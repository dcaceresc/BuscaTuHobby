namespace Application.Maintainer.PostTypes.Queries.GetPostTypeById;

public record GetPostTypeById(Guid PostTypeId) : IRequest<ApiResponse<PostTypeVM>>;

public class GetPostTypeByIdHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetPostTypeById, ApiResponse<PostTypeVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<PostTypeVM>> Handle(GetPostTypeById request, CancellationToken cancellationToken)
    {
        try
        {
            var postType = await _context.PostTypes
                .AsNoTracking()
                .Select(x => new PostTypeVM
                {
                    PostTypeId = x.PostTypeId,
                    PostTypeName = x.PostTypeName
                })
                .FirstOrDefaultAsync(x => x.PostTypeId == request.PostTypeId, cancellationToken);

            Guard.Against.NotFound(postType, $"No existe un tipo de post con la Id {request.PostTypeId}");

            return _responseService.Success(postType);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<PostTypeVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<PostTypeVM>("Ocurrió un error al obtener el tipo de post");
        }
    }
}
