namespace Application.Security.Account.Commands.AdminLogin;
public record AdminLogin(string Email, string Password, string Supplanted) : IRequest<ApiResponse>;

public class AdminLoginHandler(IApplicationDbContext context, IUtilityService utilityService, IApiResponseService responseService) : IRequestHandler<AdminLogin, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IUtilityService _utilityService = utilityService;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(AdminLogin request, CancellationToken cancellationToken)
    {
        try
        {
            var superAdmin = await _context.Users
                    .Include(x => x.UserRoles)
                    .Where(x => x.Email == request.Email && x.UserRoles.Any(ur => ur.Role.RoleName == "SuperAdmin"))
                    .FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(superAdmin, $"No existe usuario con el correo {request.Email}");

            bool userIsActive(User user) => user.IsActive;

            Guard.Against.InvalidInput(superAdmin, userIsActive, "La cuenta se encuentra desactivamente");

            bool userIsConfirmed(User user) => user.EmailConfirmed;

            Guard.Against.InvalidInput(superAdmin, userIsConfirmed, "La cuenta no ha sido confirmada");

            bool userIsLockedOut(User user) => !(user.LockoutEnabled && user.LockoutEnd > DateTime.Now);

            Guard.Against.InvalidInput(superAdmin, userIsLockedOut, "La cuenta se encuentra bloqueada por numero de intentos fallidos");

            bool usuplantedIsValid(string userSupplanted)
            {
                var suplantedUser = _context.Users.FirstOrDefault(u => u.Email == userSupplanted && u.IsActive && (!u.LockoutEnabled || (u.LockoutEnd == null || u.LockoutEnd < DateTime.Now)));
                return suplantedUser != null;
            }

            Guard.Against.InvalidInput(request.Supplanted, usuplantedIsValid, "Usuario Suplantado no existe o se encuentra bloqueado");

            bool passwordIsValid(User user) => _utilityService.VerifyHashedPassword(user.PasswordHash, request.Password);

            Guard.Against.InvalidInput(superAdmin, passwordIsValid, "Usuario o Contraseña incorrectos");

            return _responseService.Success("Usuario Validado");
        }
        catch (Common.Exceptions.ArgumentException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al iniciar sesión");
        }
    }
}
