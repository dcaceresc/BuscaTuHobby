namespace Domain.Entities;

public class FavoriteProduct : AuditableEntity
{
    public int favoriteId { get; set; }
    public Favorite Favorite { get; set; } = default!;

    public int productId { get; set; }
    public Product Product { get; set; } = default!;
}