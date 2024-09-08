namespace Domain.Entities;
public class RefreshToken : AuditableEntity
{
    private RefreshToken(Guid userId)
    {
        RefreshTokenId = Guid.NewGuid();
        RefreshTokenValue = Guid.NewGuid().ToString("N");
        RefreshTokenExpiration = DateTime.Now.AddDays(7);
        UserId = userId;
        Used = false;
    }

    public Guid RefreshTokenId { get; private set; }
    public string RefreshTokenValue { get; private set; } = null!;
    public DateTime RefreshTokenExpiration { get; private set; }
    public bool Used { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;


    public static RefreshToken Create(Guid userId)
    {
        return new RefreshToken(userId);
    }

    public void MarkAsUsed()
    {
        Used = true;
    }

}
