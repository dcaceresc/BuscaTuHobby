using Application.Common.Models;

namespace Application.Maintainer.Menus.Commands.UpdateMenu;

public record UpdateMenu(Guid MenuId, string MenuName,int MenuOrder, List<UpdateSubMenu> SubMenus) : IRequest<ApiResponse>;

public class UpdateMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Menus.FindAsync([request.MenuId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe menu con la Id {request.MenuId}");

            entity.Update(request.MenuName,request.MenuOrder);

            await _context.SaveChangesAsync(cancellationToken);

            var oldSubMenus = await _context.SubMenus.Where(x => x.MenuId == request.MenuId).ToListAsync(cancellationToken);

            foreach (var subMenu in request.SubMenus)
            {
                var subMenuEntity = oldSubMenus.FirstOrDefault(x => x.SubMenuId == subMenu.SubMenuId);
                if (subMenuEntity is null)
                {
                    subMenuEntity = entity.AddSubMenu(subMenu.SubMenuName, subMenu.SubMenuOrder);
                    _context.SubMenus.Add(subMenuEntity);
                }
                else
                {
                    subMenuEntity.Update(subMenu.SubMenuName, request.MenuId, subMenu.SubMenuOrder);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Menu actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el menu");
        }
    }
}



