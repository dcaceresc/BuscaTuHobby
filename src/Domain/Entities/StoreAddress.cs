namespace Domain.Entities;
public class StoreAddress : AuditableEntity
{
    private StoreAddress(string street,Guid storeId, Guid communeId, string? zipCode)
    {
        StoreAddressId = Guid.NewGuid();
        Street = street;
        CommuneId = communeId;
        StoreId = storeId;
        ZipCode = zipCode;
    }

    public Guid StoreAddressId { get; private set; }
    public Guid StoreId { get; private set; }
    public string Street { get; private set; } = default!;
    public Guid CommuneId { get; private set; } 
    public string? ZipCode { get; private set; }
    public bool IsActive { get; private set; }

    public Commune Commune { get; private set; } = default!;
    public Store Store { get; private set; } = default!;

    public static StoreAddress Create(string street, Guid storeId, Guid communeId, string? zipCode)
    {
        return new StoreAddress(street,storeId, communeId, zipCode);
    }

    public void Update(string street, Guid storeId, Guid communeId, string? zipCode)
    {
        Street = street;
        CommuneId = communeId;
        StoreId = storeId;
        ZipCode = zipCode;
    }
}
