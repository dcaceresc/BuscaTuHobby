namespace Domain.Entities;

public class Store : AuditableEntity
{
    private Store(string storeName, string storeWebSite)
    {
        StoreId = Guid.NewGuid();
        StoreName = storeName;
        StoreWebSite = storeWebSite;
        IsActive = true;
    }


    public Guid StoreId { get; private set; }
    public string StoreName { get; private set; } = default!;
    public string StoreWebSite { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public virtual ICollection<Inventory> Inventories { get; private set; } = default!;

    public static Store Create(string storeName, string storeWebSite)
    {
        return new Store(storeName, storeWebSite);
    }

    public void Update(string storeName, string storeWebSite)
    {
        StoreName = storeName;
        StoreWebSite = storeWebSite;
    }

    public StoreAddress AssignAddress(string street, Guid communeId, string? zipCode)
    {
        return StoreAddress.Create(street, StoreId, communeId, zipCode);
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
