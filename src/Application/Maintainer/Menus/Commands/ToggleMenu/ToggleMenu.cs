namespace Application.Maintainer.Menus.Commands.ToggleMenu;

public record ToggleMenu(Guid MenuId) : IRequest<ApiResponse>;

public class ToggleMenuHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleMenu, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleMenu request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _context.Menus.FindAsync([request.MenuId], cancellationToken);

            Guard.Against.NotFound(entity, $"No existe menu con la id {request.MenuId}");

            entity.ToggleActive();

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