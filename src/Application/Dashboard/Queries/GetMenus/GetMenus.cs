namespace Application.Dashboard.Queries.GetMenus;
public record GetMenus : IRequest<ApiResponse<List<MenuDto>>>;

public class GetMenusHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<GetMenus, ApiResponse<List<MenuDto>>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<List<MenuDto>>> Handle(GetMenus request, CancellationToken cancellationToken)
    {
		try
		{
            var menus = await _context.Menus
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.MenuOrder)
                .Select(x => new MenuDto
                {
                    MenuName = x.MenuName,
                    MenuOrder = x.MenuOrder,
                    MenuSlug = x.MenuSlug
                })
                .ToListAsync(cancellationToken);

            foreach (var menu in menus)
            {
                menu.SubMenus = _context.SubMenus
                    .AsNoTracking()
                    .Where(x => x.Menu.MenuName == menu.MenuName && x.IsActive)
                    .Select(x => new SubMenuDto
                    {
                        SubMenuName = x.SubMenuName,
                        SubMenuOrder = x.SubMenuOrder,
                        SubMenuSlug = x.SubMenuSlug
                    })
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
