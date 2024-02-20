namespace Domain.Entities;

public class Favorite : AuditableEntity
{
    public Guid FavoriteId { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = default!;


}