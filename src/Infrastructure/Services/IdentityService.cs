using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

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

    public bool IsValidEmail(string email)
    {
        // Use a regular expression to validate the email format
        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(email);
    }
}
