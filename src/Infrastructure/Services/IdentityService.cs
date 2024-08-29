using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;
public class IdentityService
{
    public string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<IdentityService>();
        return passwordHasher.HashPassword(this, password);
    }

    public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var passwordHasher = new PasswordHasher<IdentityService>();
        return passwordHasher.VerifyHashedPassword(this, hashedPassword, providedPassword);
    }
}
