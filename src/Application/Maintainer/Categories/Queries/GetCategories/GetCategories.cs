namespace Application.Maintainer.Categories.Queries.GetCategories;
public record GetCategories : IRequest<ApiResponse<List<CategoryDto>>>;

public class GetSubCategoriesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCategories, ApiResponse<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategories request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _context.Categories
            .Include(x => x.Group)
            .AsNoTracking()
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(categories);

        }
        catch (Exception)
        {
            return _responseService.Fail<List<CategoryDto>>("Error al obtener las categorias");
        }
    }
}


