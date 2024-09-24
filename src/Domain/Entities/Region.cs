namespace Domain.Entities;
public class Region : AuditableEntity
{
    private Region(string regionName)
    {
        RegionId = Guid.NewGuid();
        RegionName = regionName;
        IsActive = true;
    }

    public Guid RegionId { get; private set; }
    public string RegionName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public ICollection<City> Cities { get; private set; } = default!;

    public static Region Create(string regionName)
    {
        return new Region(regionName);
    }

    public void Update(string regionName)
    {
        RegionName = regionName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
