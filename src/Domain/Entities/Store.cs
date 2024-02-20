namespace Domain.Entities;

public class Store : AuditableEntity
{
    private Store(string storeName, string storeAddress, string storeWebSite)
    {
        StoreId = Guid.NewGuid();
        StoreName = storeName;
        StoreAddress = storeAddress;
        StoreWebSite = storeWebSite;
        IsActive = true;
    }


    public Guid StoreId { get; private set; }
    public string StoreName { get; private set; } = default!;
    public string StoreAddress { get; private set; } = default!;
    public string StoreWebSite { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public virtual ICollection<Inventory> Inventories { get; private set; } = default!;

    public static Store Create(string storeName, string storeAddress, string storeWebSite)
    {
        return new Store(storeName, storeAddress, storeWebSite);
    }

    public void Update(string storeName, string storeAddress, string storeWebSite)
    {
        StoreName = storeName;
        StoreAddress = storeAddress;
        StoreWebSite = storeWebSite;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
