namespace Domain.Entities;
public class User : AuditableEntity
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;

    public ICollection<Review> Reviews { get; set; } = default!;
    public ICollection<Favorite> Favorites { get; set; } = default!;
    public ICollection<UserRole> UserRoles { get; set; } = default!;
}
