using Application.Common.Interfaces;
using System.Security.Claims;

namespace WebAPI.Services;

public class AuthenticationService(IHttpContextAccessor httpContextAccessor) : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
