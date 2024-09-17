namespace Application.Security.Account.Commands.ConfirmEmail;
public record ConfirmEmail(Guid UserId, string Token) : IRequest<ApiResponse>;

public class ConfirmEmailHandler(IApplicationDbContext context, IApiResponseService responseService, IUtilityService utilityService) : IRequestHandler<ConfirmEmail, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IUtilityService _utilityService = utilityService;

    public async Task<ApiResponse> Handle(ConfirmEmail request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FindAsync([request.UserId], cancellationToken);

            Guard.Against.NotFound(user, "Usuario no encontrado");

            if (!_utilityService.ValidateEmailConfirmationToken(user, request.Token))
                return _responseService.Fail("Token de confirmación inválido");

            user.ConfirmEmail();

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Tu correo electrónico ya ha sido confirmado. Ahora puedes iniciar sesión en BuscaTuHobby");
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("Error al confirmar el email");
        }
    }
}
