namespace Application.Maintainer.Categories.Queries.GetCategories;
public record GetCategoriesQuery : IRequest<ApiResponse<List<CategoryDto>>>;

public class GetSubCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCategoriesQuery, ApiResponse<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
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


