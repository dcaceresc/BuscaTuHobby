namespace Application.Maintainer.Menus.Commands.ToggleSubMenu;
public record ToggleSubMenu(Guid MenuId, Guid SubMenuId) : IRequest<ApiResponse>;

public class ToggleSubMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleSubMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleSubMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var menu = await _context.Menus.FindAsync([request.MenuId], cancellationToken);

            Guard.Against.NotFound(menu, $"No existe menu con la id {request.MenuId}");

            var entity = await _context
                .SubMenus
                .FirstOrDefaultAsync(x => x.SubMenuId == request.SubMenuId, cancellationToken);

            Guard.Against.NotFound(entity, $"No existe sub menu con la id {request.SubMenuId}");

            entity.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Sub menu actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el sub menu");
        }
    }
}
