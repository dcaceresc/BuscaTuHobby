namespace Application.Common.Interfaces;

public interface IAuthenticationService
{
    string? UserName { get; }
    public string CreateAccessToken(string username, IList<string> roles);
    public Task AuthenticationLOGMOD(string userName, string password);
}