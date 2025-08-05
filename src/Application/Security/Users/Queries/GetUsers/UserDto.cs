namespace Application.Security.Users.Queries.GetUsers;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public string LockoutEnd { get; set; } = default!;
    public bool LockoutEnabled { get; set; }
    public IList<string> RoleNames { get; set; } = default!;
    public bool IsActive { get; set; }
}