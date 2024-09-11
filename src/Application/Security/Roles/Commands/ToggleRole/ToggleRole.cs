namespace Application.Security.Roles.Commands.ToggleRole;
public record ToggleRole(Guid RoleId) : IRequest<ApiResponse>;

public class ToggleRoleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<ToggleRole, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(ToggleRole request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles.FindAsync([request.RoleId], cancellationToken);

            Guard.Against.NotFound(role, $"No existe permiso con la Id {request.RoleId}");

            role.ToggleActive();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Permiso actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al actualizar el permiso");
        }
    }
}