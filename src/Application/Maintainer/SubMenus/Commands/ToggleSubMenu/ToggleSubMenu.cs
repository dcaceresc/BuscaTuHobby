namespace Application.Maintainer.SubMenus.Commands.ToggleCategory;
public record ToggleSubMenu(Guid SubMenuId) : IRequest<ApiResponse>;

public class ToggleSubMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleSubMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleSubMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.SubMenus.FindAsync([request.SubMenuId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe sub menu con la Id {request.SubMenuId}");

            entity.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("El estado de la sub menu ha sido actualizado exitosamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el estado de la sub menu");
        }
    }
}