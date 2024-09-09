namespace Application.Common.Interfaces;
public interface IIdentityService
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
    bool IsValidEmail(string email);
}
