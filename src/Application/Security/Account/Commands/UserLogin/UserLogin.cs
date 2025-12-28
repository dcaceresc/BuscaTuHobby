namespace Application.Security.Account.Commands.UserLogin;
public record UserLogin(string Email, string Password) : IRequest<ApiResponse>;

public class UserLoginHandler(IApplicationDbContext context, IApiResponseService responseService, IUtilityService utilityService) : IRequestHandler<UserLogin, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IUtilityService _utilityService = utilityService;

    public async Task<ApiResponse> Handle(UserLogin request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            Guard.Against.NotFound(user, $"No existe en el sistema usuario con el correo {request.Email}");

            bool userIsActive(User user) => user.IsActive;

            Guard.Against.InvalidInput(user, userIsActive, "La cuenta se encuentra desactivamente");

            bool userIsConfirmed(User user) => user.EmailConfirmed;

            Guard.Against.InvalidInput(user, userIsConfirmed, "La cuenta no ha sido confirmada");

            bool userIsLockedOut(User user) => !(user.LockoutEnabled && user.LockoutEnd > DateTime.Now);

            Guard.Against.InvalidInput(user, userIsLockedOut, "La cuenta se encuentra bloqueada por numero de intentos fallidos");

            bool passwordIsValid(User user) => _utilityService.VerifyHashedPassword(user.PasswordHash, request.Password);

            Guard.Against.InvalidInput(user, passwordIsValid, "Usuario o Contraseña incorrectos");

            user.LoginSuccess();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Usuario Validado");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Common.Exceptions.ArgumentException ex)
        {
            var user = await _context.Users.FirstAsync(x => x.Email == request.Email, cancellationToken);

            user.LoginFail();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al iniciar sesión");
        }
    }
}
