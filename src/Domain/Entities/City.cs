namespace Domain.Entities;
public class City : AuditableEntity
{
    private City(string cityName, Guid regionId)
    {
        CityId = Guid.NewGuid();
        CityName = cityName;
        RegionId = regionId;
        IsActive = true;
    }

    public Guid CityId { get; private set; }
    public string CityName { get; private set; } = default!;
    public Guid RegionId { get; private set; }
    public bool IsActive { get; private set; }

    public Region Region { get; private set; } = default!;
    public ICollection<StoreAddress> StoreAddresses { get; private set; } = default!;

    public static City Create(string cityName, Guid regionId)
    {
        return new City(cityName,regionId);
    }

    public void Update(string cityName, Guid regionId)
    {
        CityName = cityName;
        RegionId = regionId;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
