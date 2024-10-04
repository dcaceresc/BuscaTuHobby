namespace Domain.Entities;
public class Commune : AuditableEntity
{
    private Commune(string communeName, Guid regionId)
    {
        CommuneId = Guid.NewGuid();
        CommuneName = communeName;
        RegionId = regionId;
        IsActive = true;
    }

    public Guid CommuneId { get; private set; }
    public string CommuneName { get; private set; } = default!;
    public Guid RegionId { get; private set; }
    public bool IsActive { get; private set; }

    public Region Region { get; private set; } = default!;
    public ICollection<StoreAddress> StoreAddresses { get; private set; } = default!;

    public static Commune Create(string communeName, Guid regionId)
    {
        return new Commune(communeName, regionId);
    }

    public void Update(string communeName, Guid regionId)
    {
        CommuneName = communeName;
        RegionId = regionId;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
