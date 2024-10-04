namespace Application.Maintainer.Categories.Queries.GetCategories;

public record GetCategories : IRequest<ApiResponse<List<CategoryDto>>>;

public class GetCategoriesHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetCategories, ApiResponse<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<CategoryDto>>> Handle(GetCategories request, CancellationToken cancellationToken)
    {
        try
        {
            var scales = await _context.Categories
                .AsNoTracking()
                .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return _responseService.Success(scales);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<CategoryDto>>("Ocurrió un error al obtener las categorias");
        }
    }
}

