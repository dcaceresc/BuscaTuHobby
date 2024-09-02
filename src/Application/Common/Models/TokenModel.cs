namespace Application.Common.Models;
public class TokenModel
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
