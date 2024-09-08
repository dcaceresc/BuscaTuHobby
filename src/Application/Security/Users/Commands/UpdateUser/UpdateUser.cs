namespace Application.Security.Users.Commands.UpdateUser;
public record UpdateUser : IRequest<ApiResponse>
{
    public string Email { get; init; } = default!;
    public string PasswordHash { get; init; } = default!;

}
