namespace Domain.Entities;

public class Favorite : AuditableEntity
{
    public int id { get; set; }
    public string userId { get; set; } = default!;

    public ICollection<FavoriteProduct> FavoriteProducts { get; set; } = default!;

}