namespace Domain.Entities;

public class FavoriteGunpla : AuditableEntity
{
    public int favoriteId { get; set; }
    public Favorite Favorite { get; set; } = default!;

    public int gunplaId { get; set; }
    public Gunpla Gunpla { get; set; } = default!;
}