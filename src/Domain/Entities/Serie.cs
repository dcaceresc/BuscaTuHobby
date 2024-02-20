namespace Domain.Entities;

public class Serie : AuditableEntity
{
    private Serie(string serieName, Guid franchiseId)
    {
        SerieId = Guid.NewGuid();
        SerieName = serieName;
        FranchiseId = franchiseId;
        IsActive = true;
    }

    public Guid SerieId { get; private set; }
    public string SerieName { get; private set; } = default!;
    public Guid FranchiseId { get; private set; }
    public bool IsActive { get; private set; }

    public virtual Franchise Franchise { get; private set; } = default!;


    public static Serie Create(string serieName, Guid franchiseId)
    {
        return new Serie(serieName, franchiseId);
    }

    public void Update(string serieName, Guid franchiseId)
    {
        SerieName = serieName;
        FranchiseId = franchiseId;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
