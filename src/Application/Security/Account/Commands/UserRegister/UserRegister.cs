namespace Application.Security.Account.Commands.UserRegister;
public record UserRegister : IRequest<ApiResponse>
{
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string ConfirmPassword { get; init; } = default!;
    public bool AcceptTerms { get; init; }
}

public class UserRegisterHandler(IApplicationDbContext context, IApiResponseService responseService, IIdentityService identityService) : IRequestHandler<UserRegister, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IIdentityService _identityService = identityService;

    public async Task<ApiResponse> Handle(UserRegister request, CancellationToken cancellationToken)
    {
        try
        {
            if (!_identityService.IsValidEmail(request.Email))
                return _responseService.Fail("Email inválido");

            if (request.Password != request.ConfirmPassword)
                return _responseService.Fail("Las contraseñas no coinciden");

            if (!request.AcceptTerms)
                return _responseService.Fail("Debe aceptar los términos y condiciones");

            bool userExists(string email) => !_context.Users.Any(x => x.Email == email);

            Guard.Against.InvalidInput(request.Email, userExists,"El email ya existe");

            var user = User.Create(request.Email, _identityService.HashPassword(request.Password));

            _context.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            var role = await _context.Roles.SingleOrDefaultAsync(r => r.RoleName == "User", cancellationToken);

            Guard.Against.NotFound(role, $"No existe el rol User");

            var userRole = user.AssignRole(role.RoleId);

            _context.UserRoles.Add(userRole);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Usuario creado exitosamente");

        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Common.Exceptions.ArgumentException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al crear el usuario");
        }
    }

    
}
