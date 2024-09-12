namespace Application.Security.Users.Commands.CreateUser;
public record CreateUser(string Email, IList<string> RoleIds) : IRequest<ApiResponse>;

public class CreateUserHandler(IApplicationDbContext context, IApiResponseService responseService, IUtilityService utilityService) : IRequestHandler<CreateUser, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IUtilityService _utilityService = utilityService;

    public async Task<ApiResponse> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        try
        {
            var user = User.Create(request.Email, _utilityService.HashPassword(_utilityService.GenerateRandomString(20)));

            _context.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var roleId in request.RoleIds)
            {
                var userRole = user.AssignRole(Guid.Parse(roleId));

                _context.UserRoles.Add(userRole);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Usuario creado exitosamente");

        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear el usuario");
        }
    }
}

