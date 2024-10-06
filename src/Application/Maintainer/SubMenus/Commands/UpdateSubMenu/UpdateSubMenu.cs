namespace Application.Maintainer.SubMenus.Commands.UpdateCategory;

public record UpdateSubMenu(Guid SubMenuId, string SubMenuName, Guid MenuId, int SubMenuOrder) : IRequest<ApiResponse>;

public class UpdateSubMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateSubMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateSubMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.SubMenus.FindAsync([request.SubMenuId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe sub menu con la Id {request.SubMenuId}");

            entity.Update(request.SubMenuName, request.MenuId,request.SubMenuOrder);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("SubMenu actualizado exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar la sub menu");
        }
    }
}
