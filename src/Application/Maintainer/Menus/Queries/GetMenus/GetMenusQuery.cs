namespace Application.Maintainer.Menus.Queries.GetMenus;

public class GetMenus : IRequest<ApiResponse<List<MenuDto>>>;

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
            .OrderBy(x => x.MenuOrder)
            .Select(x => new MenuDto
            {
                MenuId = x.MenuId,
                MenuName = x.MenuName,
                MenuSlug = x.MenuSlug,
                SubMenus = x.SubMenus
                    .OrderBy(sm => sm.SubMenuOrder)
                    .Select(sm => sm.SubMenuName)
                    .ToList()
            })
            .ToListAsync(cancellationToken);


            return _responseService.Success(menus);
        }
        catch (Exception)
        {
            return _responseService.Fail<List<MenuDto>>("No se pudo obtener los menus");
        }
    }
}