namespace Application.Dashboard.Queries.GetMenus;
public record GetMenus : IRequest<ApiResponse<List<MenuDto>>>;

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
                .Where(x => x.IsActive)
                .OrderBy(x => x.MenuOrder)
                .ProjectTo<MenuDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            foreach (var menu in menus)
            {
                menu.SubMenus = _context.SubMenus
                    .AsNoTracking()
                    .Where(x => x.Menu.MenuName == menu.MenuName && x.IsActive)
                    .ProjectTo<SubMenuDto>(_mapper.ConfigurationProvider)
                    .ToList();
            }


            return _responseService.Success(menus);
        }
		catch (Exception)
		{
            return _responseService.Fail<List<MenuDto>>("No se pudo obtener los menus");
        }
    }
}
