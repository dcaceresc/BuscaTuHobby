namespace Application.Maintainer.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<ApiResponse<CategoryVM>>;

public class GetSubCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCategoryByIdQuery, ApiResponse<CategoryVM>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<CategoryVM>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _context.Categories
            .Where(x => x.CategoryId == request.CategoryId)
            .AsNoTracking()
            .ProjectTo<CategoryVM>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(category, $"No existe categoria con la Id {request.CategoryId}");

            return _responseService.Success(category);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<CategoryVM>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<CategoryVM>("Error al obtener la categoria");
        }

    }
}
