using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;
public class UtilityService : IUtilityService
{
    private const string _chars = "CfaG2eMwhG@*pukixPL_ti@d_AaG*w6oo6G9hwuZu*EDAA_3aWRGKoo3cKxU";


    public string HashPassword(string password)
    {
        var passwordHasher = new PasswordHasher<UtilityService>();
        return passwordHasher.HashPassword(this, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var passwordHasher = new PasswordHasher<UtilityService>();

        var result = passwordHasher.VerifyHashedPassword(this, hashedPassword, providedPassword);

        return result == PasswordVerificationResult.Success;
    }

    public bool IsValidEmail(string email)
    {
        // Use a regular expression to validate the email format
        string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        Regex regex = new(pattern);
        return regex.IsMatch(email);
    }

    public string GenerateRandomString(int length)
    {
        Random random = new();
        return new string(Enumerable.Repeat(_chars, length)
       .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
