namespace Domain.Entities;
public class Role(Guid roleId, string roleName) : AuditableEntity
{
    public Guid RoleId { get; private set; } = roleId;
    public string RoleName { get; private set; } = roleName;
    public ICollection<UserRole> UserRoles { get; set; } = default!;


    public static Role Create(string roleName)
    {
        return new Role(Guid.NewGuid(), roleName);
    }
}
