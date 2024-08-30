using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;
public class IdentityService : IIdentityService
{
    public string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<IdentityService>();
        return passwordHasher.HashPassword(this, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var passwordHasher = new PasswordHasher<IdentityService>();

        var result = passwordHasher.VerifyHashedPassword(this, hashedPassword, providedPassword);

        return result == PasswordVerificationResult.Success;
    }
}
