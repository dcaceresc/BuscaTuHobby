namespace Domain.Entities;
public class User : AuditableEntity
{
    public User(string userName, string passwordHash)
    {
        UserId = Guid.NewGuid();
        UserName = userName;
        Email = userName;
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString();
        EmailConfirmed = false;
        LockoutEnabled = true;
        AccessFailedCount = 0;
    }



    public Guid UserId { get; private set; } 
    public string UserName { get; private set; } 
    public string Email { get; private set; } 
    public string PasswordHash { get; private set; } 
    public string SecurityStamp { get; private set; } 
    public bool EmailConfirmed { get; private set; } 
    public DateTime? LockoutEnd { get; set; } 
    public bool LockoutEnabled { get; set; } 
    public int AccessFailedCount { get; set; } 
    public DateTime? LastLoginDate { get; set; }

    public ICollection<Review> Reviews { get; set; } = default!;
    public ICollection<Favorite> Favorites { get; set; } = default!;
    public ICollection<UserRole> UserRoles { get; set; } = default!;


    public static User Create(string userName, string passwordHash)
    {
        return new User(userName, passwordHash);
    }

    public UserRole AssignRole(Guid roleId)
    {
        return new UserRole(UserId, roleId);
    }
}
