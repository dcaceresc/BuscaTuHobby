namespace Domain.Entities;

public class Inventory : AuditableEntity
{
    public int id { get; set; }
    public int storeId { get; set; }
    public int productId { get; set; }
    public int price { get; set; }
    public bool active { get; set; }

    public virtual Product Product { get; set; } = default!;
    public virtual Store Store { get; set; } = default!;
}