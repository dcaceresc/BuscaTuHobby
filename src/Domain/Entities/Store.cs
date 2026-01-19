namespace Domain.Entities;

public class Store : AuditableEntity
{
    private Store(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        StoreId = Guid.NewGuid();
        StoreName = storeName;
        StoreIcon = storeIcon;
        StoreWebSite = storeWebSite;
        StoreOrder = storeOrder;
        StoreSlug = storeSlug;
        IsActive = true;
    }
    
    public Guid StoreId { get; private set; }
    public string StoreName { get; private set; } = default!;
    public string StoreIcon { get; private set; } = default!;
    public string StoreWebSite { get; private set; } = default!;
    public int StoreOrder { get; private set; }
    public string StoreSlug { get; private set; } = default!;
    public bool IsActive { get; private set; }
    
    public virtual ICollection<Inventory> Inventories { get; private set; } = default!;
    public virtual ICollection<StoreAddress> StoreAddresses { get; private set; } = default!;

    public static Store Create(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        return new Store(storeName, storeWebSite, storeIcon, storeOrder, storeSlug);
    }

    public void Update(string storeName, string storeWebSite, string storeIcon, int storeOrder, string storeSlug)
    {
        StoreName = storeName;
        StoreWebSite = storeWebSite;
        StoreIcon = storeIcon;
        StoreOrder = storeOrder;
        StoreSlug = storeSlug;
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
