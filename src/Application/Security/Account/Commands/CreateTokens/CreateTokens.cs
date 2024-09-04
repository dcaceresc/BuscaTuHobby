using Application.Common.Models;

namespace Application.Security.Account.Commands.CreateTokens;
public record CreateTokens(string UserName) : IRequest<ApiResponse<TokenModel>>;

public class CreateTokensHandler(IApplicationDbContext context, IAuthenticationService authenticationService, IIdentityService identityService, IApiResponseService responseService)
    : IRequestHandler<CreateTokens, ApiResponse<TokenModel>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly IIdentityService _identityService = identityService;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<TokenModel>> Handle(CreateTokens request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.Where(x => x.UserName == request.UserName).FirstOrDefaultAsync(cancellationToken);

            Guard.Against.NotFound(user, $"No existe usuario con el nombre de usuario {request.UserName}");

            var refreshToken = RefreshToken.Create(user.UserId);

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            var oldRefreshTokens = await _context.RefreshTokens
                    .Where(x => x.UserId == user.UserId && x.RefreshTokenExpiration < DateTime.Now)
            .ToListAsync(cancellationToken);

            _context.RefreshTokens.RemoveRange(oldRefreshTokens);

            var roles = await _context.Users
                    .Include(x => x.UserRoles)
                    .Where(x => x.UserName == request.UserName).Select(x => x.UserRoles.Select(x => x.Role.RoleName).First())
                    .ToListAsync(cancellationToken);

            return _responseService.Success(new TokenModel
            {
                AccessToken = _authenticationService.CreateAccessToken(request.UserName, roles),
                RefreshToken = refreshToken.RefreshTokenValue
            });
        }
        catch (Common.Exceptions.NotFoundException ex)
        {
            return _responseService.Fail<TokenModel>(ex.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail<TokenModel>("Error al crear los tokens");
        }
    }
}
