namespace Domain.Entities;
public class Role : AuditableEntity
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = default!;
    public ICollection<UserRole> UserRoles { get; set; } = default!;
}
