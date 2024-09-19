using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
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

    public string GenerateEmailConfirmationToken(User user)
    {
        // Concatenar el UserId y el SecurityStamp
        var tokenInput = $"{user.UserId}:{user.SecurityStamp}";

        // Generar un hash basado en el UserId y SecurityStamp para mayor seguridad
        var tokenBytes = Encoding.UTF8.GetBytes(tokenInput);
        var hashedToken = Convert.ToBase64String(SHA256.Create().ComputeHash(tokenBytes));

        // Codificar el token en formato URL
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(hashedToken));
    }

    public bool ValidateEmailConfirmationToken(User user, string token)
    {
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        // Volver a generar el hash a partir de los valores originales
        var expectedTokenInput = $"{user.UserId}:{user.SecurityStamp}";
        var expectedTokenBytes = Encoding.UTF8.GetBytes(expectedTokenInput);
        var expectedToken = Convert.ToBase64String(SHA256.Create().ComputeHash(expectedTokenBytes));

        // Comparar el token generado con el token proporcionado
        return decodedToken == expectedToken;
    }

    public async Task SaveImagen(string image, Guid name)
    {
        string imagesFolderPath = "Imagenes";
        if (!Directory.Exists(imagesFolderPath))
        {
            Directory.CreateDirectory(imagesFolderPath);
        }

        string base64Image = image;
        byte[] imageBytes = Convert.FromBase64String(base64Image);
        string imageName = $"{name}.jpg";
        string imagePath = Path.Combine(imagesFolderPath, imageName);
        await File.WriteAllBytesAsync(imagePath, imageBytes);
    }
}
