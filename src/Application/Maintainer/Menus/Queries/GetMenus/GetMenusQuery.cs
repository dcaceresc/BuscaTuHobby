namespace Application.Maintainer.Menus.Queries.GetMenus;

public class GetMenus : IRequest<ApiResponse<List<MenuDto>>>;

public class GetMenusHandler(IApplicationDbContext context, IMapper mapper, IApiResponseService responseService) : IRequestHandler<GetMenus, ApiResponse<List<MenuDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<MenuDto>>> Handle(GetMenus request, CancellationToken cancellationToken)
    {
        try
        {
            var menus = await _context.Menus
            .AsNoTracking()
            .OrderBy(x => x.MenuOrder)
            .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

            return _responseService.Success(menus);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<MenuDto>>("No se pudo obtener los menus");
        }
    }
}