namespace Domain.Entities;
public class StoreAddress : AuditableEntity
{
    private StoreAddress(string street,Guid storeId, Guid cityId, string? zipCode)
    {
        StoreAddressId = Guid.NewGuid();
        Street = street;
        CityId = cityId;
        StoreId = storeId;
        ZipCode = zipCode;
    }

    public Guid StoreAddressId { get; private set; }
    public Guid StoreId { get; private set; }
    public string Street { get; private set; } = default!;
    public Guid CityId { get; private set; } 
    public string? ZipCode { get; private set; }
    public bool IsActive { get; private set; }

    public City City { get; private set; } = default!;
    public Store Store { get; private set; } = default!;

    public static StoreAddress Create(string street, Guid storeId, Guid cityId, string? zipCode)
    {
        return new StoreAddress(street,storeId, cityId, zipCode);
    }

    public void Update(string street, Guid storeId, Guid cityId, string? zipCode)
    {
        Street = street;
        CityId = cityId;
        StoreId = storeId;
        ZipCode = zipCode;
    }
}
