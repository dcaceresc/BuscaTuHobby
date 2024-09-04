using Application.Common.Models;

namespace Application.Security.Account.Commands.UpdateRefreshToken;
public record UpdateRefreshToken(string RefreshToken) : IRequest<ApiResponse<TokenModel>>;

public class UpdateRefreshTokenHandler(IApplicationDbContext context, IApiResponseService responseService, IAuthenticationService authenticationService) : IRequestHandler<UpdateRefreshToken, ApiResponse<TokenModel>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;
    private readonly IAuthenticationService _authenticationService = authenticationService;

    public async Task<ApiResponse<TokenModel>> Handle(UpdateRefreshToken request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.RefreshTokenValue == request.RefreshToken, cancellationToken);

            if (refreshToken is null || refreshToken.RefreshTokenExpiration <= DateTime.Now)
                Guard.Against.NotFound(refreshToken, $"El refreshToken {request.RefreshToken} no existe");


            if (refreshToken.Used)
                Guard.Against.ForbiddenAccess($"El refreshToken {request.RefreshToken} ya fue usado");

            refreshToken.MarkAsUsed();

            var user = await _context.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.UserId == refreshToken.UserId, cancellationToken);

            Guard.Against.NotFound(user, $"El usuario con el refreshToken {request.RefreshToken} no existe");

            var accessToken = _authenticationService.CreateAccessToken(user.UserName, user.UserRoles.Select(x => x.Role.RoleName).ToList());

            var newRefreshToken = RefreshToken.Create(user.UserId);

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success(new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.RefreshTokenValue
            });
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<TokenModel>(ex.Message);
        }
        catch (ForbiddenAccessException ex)
        {
            return _responseService.Fail<TokenModel>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<TokenModel>("Ah ocurrido un error al autenticar el usuario");
        }
    }
}
