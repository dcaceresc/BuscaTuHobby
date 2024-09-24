namespace Domain.Entities;

public class Inventory : AuditableEntity
{
    private Inventory(Guid storeId, Guid productId, int price)
    {
        InventoryId = Guid.NewGuid();
        StoreId = storeId;
        ProductId = productId;
        Price = price;
        IsActive = true;
    }

    public Guid InventoryId { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Price { get; private set; }
    public bool IsActive { get; private set; }

    public virtual Product Product { get; private set; } = default!;
    public virtual Store Store { get; private set; } = default!;


    public static Inventory Create(Guid productId, Guid storeId, int price)
    {
        return new Inventory(storeId, productId, price);
    }

    public void Update(Guid productId, Guid storeId, int price)
    {
        StoreId = storeId;
        ProductId = productId;
        Price = price;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}