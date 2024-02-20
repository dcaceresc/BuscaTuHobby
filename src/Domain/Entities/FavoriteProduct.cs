namespace Domain.Entities;

public class FavoriteProduct : AuditableEntity
{
    public Guid FavoriteId { get; set; }
    public Favorite Favorite { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;
}