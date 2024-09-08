namespace Application.Security.Users.Commands.ToggleUser;
public record ToggleUser(Guid UserId) : IRequest<ApiResponse>;


