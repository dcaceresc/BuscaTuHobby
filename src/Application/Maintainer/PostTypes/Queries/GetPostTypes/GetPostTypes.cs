namespace Application.Maintainer.PostTypes.Queries.GetPostTypes;

public record GetPostTypes : IRequest<ApiResponse<List<PostTypeDto>>>;

public class GetPostTypesHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetPostTypes, ApiResponse<List<PostTypeDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<PostTypeDto>>> Handle(GetPostTypes request, CancellationToken cancellationToken)
    {
        try
        {
            var postTypes = await _context.PostTypes
                .AsNoTracking()
                .Select(x => new PostTypeDto
                {
                    PostTypeId = x.PostTypeId,
                    PostTypeName = x.PostTypeName,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return _responseService.Success(postTypes);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<PostTypeDto>>("Ocurrió un error al obtener los tipos de post");
        }
    }
}
