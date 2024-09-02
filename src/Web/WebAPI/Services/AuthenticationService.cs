using Application.Common.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace WebAPI.Services;

public class AuthenticationService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : IAuthenticationService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IConfiguration _configuration = configuration;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string CreateAccessToken(string username, IList<string> roles)
    {

        // Crear las claims con el nombre de usuario
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, username) };

        // Agregar las claims de roles
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Configurar la clave secreta y las credenciales de firma
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        // Configurar las opciones del token JWT
        var tokenOptions = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:ExpirationTime"]!)),
            signingCredentials: signinCredentials);

        // Generar el token JWT
        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        return tokenString;
    }

    public async Task AuthenticationLOGMOD(string userName, string password)
    {
        string url_login = "https://media.fen.uchile.cl/logmod/proc_login.php";

        var nvc = new List<KeyValuePair<string, string>>
    {
        new("username", userName),
        new("password", password)
    };
        var client = new HttpClient();

        url_login = url_login.Trim();
        if (string.IsNullOrEmpty(url_login))
            throw new ArgumentNullException(url_login.GetType().Name, "Se requiere la url para realizar la conexión con LOGMOD");

        HttpResponseMessage response = await client.PostAsync(url_login, new FormUrlEncodedContent(nvc));
        string Result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error de conexion con el servidor.");

        if (string.IsNullOrEmpty(Result))
            throw new Exception("Error de conexion con LOGMOD. Retorno vacío.");

        var sid = Result.Substring(Result.IndexOf("sid=") + 4, Result.IndexOf("&username") - Result.IndexOf("sid=") - 4);

        if (string.IsNullOrEmpty(sid))
            throw new Exception("Error de conexion con LOGMOD. Hay problema con el usuario.");


        string url_verification = $"http://media.fen.uchile.cl/logmod/verificacion.php?username={userName}&sid={sid}";

        url_verification = url_verification.Trim();
        if (string.IsNullOrEmpty(url_verification))
            throw new ArgumentNullException(url_verification.GetType().Name, "Se requiere la url para efectuar la comunicación con LOGMOD.");

        response = await client.GetAsync(url_verification);

        Result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception("Error de conexion con el servidor.");

        if (string.IsNullOrEmpty(Result))
            throw new Exception("Error de conexion con LOGMOD. Retorno vacío.");
    }
}
