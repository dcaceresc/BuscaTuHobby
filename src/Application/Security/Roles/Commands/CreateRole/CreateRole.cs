namespace Application.Security.Roles.Commands.CreateRole;
public record CreateRole(string RoleName) : IRequest<ApiResponse>;

public class CreateRoleHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateRole, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateRole request, CancellationToken cancellationToken)
    {
        try
        {
            var role = Role.Create(request.RoleName);

            _context.Roles.Add(role);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Permiso creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear el permiso");
        }
    }
}
