namespace Application.Security.Account.Commands.UserLogin;
public record UserLogin(string Email, string Password) : IRequest<ApiResponse>;

public class UserLoginHandler(IApplicationDbContext context, IApiResponseService responseService, Identityser) : IRequestHandler<UserLogin, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UserLogin request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.email);

        if (user is null)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.password, false);

        if (!result.Succeeded)
            return null;

        return new ApiResponse { Success = true, Message = "Usuario autenticado correctamente" };
    }
}
