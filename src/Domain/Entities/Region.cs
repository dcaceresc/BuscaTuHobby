namespace Domain.Entities;
public class Region : AuditableEntity
{
    private Region(string regionName, int regionOrder)
    {
        RegionId = Guid.NewGuid();
        RegionName = regionName;
        RegionOrder = regionOrder;
        IsActive = true;
    }

    public Guid RegionId { get; private set; }
    public string RegionName { get; private set; } = default!;
    public int RegionOrder { get; private set; }
    public bool IsActive { get; private set; }

    public ICollection<Commune> Communes { get; private set; } = default!;

    public static Region Create(string regionName, int regionOrder)
    {
        return new Region(regionName, regionOrder);
    }

    public void Update(string regionName, int regionOrder)
    {
        RegionName = regionName;
        RegionOrder = regionOrder;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
