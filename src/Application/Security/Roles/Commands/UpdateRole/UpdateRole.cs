namespace Application.Security.Roles.Commands.UpdateRole;
public record UpdateRole(Guid RoleId, string RoleName) : IRequest<ApiResponse>;

public class UpdateRoleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateRole, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateRole request, CancellationToken cancellationToken)
    {
        try
        {
            var role = await _context.Roles.FindAsync([request.RoleId], cancellationToken);

            Guard.Against.NotFound(role, $"No existe permiso con la Id {request.RoleId}");

            role.Update(request.RoleName);

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
