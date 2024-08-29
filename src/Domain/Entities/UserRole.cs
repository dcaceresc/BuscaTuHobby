namespace Domain.Entities;
public class UserRole(Guid userId, Guid roleId) : AuditableEntity
{
    public Guid UserId { get; private set; } = userId;
    public User User { get; set; } = default!;
    public Guid RoleId { get; private set; } = roleId;
    public Role Role { get; set; } = default!;


    public static UserRole Create(Guid userId, Guid roleId)
    {
        return new UserRole(userId, roleId);
    }
}
