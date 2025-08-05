namespace Application.Security.Users.Queries.GetUserById;

public class UserVM
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public bool EmailConfirmed { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public IList<Guid> RoleIds { get; set; } = default!;
}