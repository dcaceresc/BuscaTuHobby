namespace Application.Maintainer.SubMenus.Commands.CreateSubMenu;

public record CreateSubMenu(string SubMenuName, Guid MenuId) : IRequest<ApiResponse>;

public class CreateSubMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateSubMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateSubMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = SubMenu.Create(request.SubMenuName, request.MenuId);

            _context.SubMenus.Add(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("SubMenu creado exitosamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear la sub menu");
        }
    }
}
