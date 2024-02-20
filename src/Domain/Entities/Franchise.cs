namespace Domain.Entities;
public class Franchise : AuditableEntity
{
    private Franchise(string franchiseName)
    {
        FranchiseId = Guid.NewGuid();
        FranchiseName = franchiseName;
        IsActive = true;
    }


    public Guid FranchiseId { get; private set; }
    public string FranchiseName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<Serie> Series { get; private set; } = default!;
    public virtual ICollection<Product> Products { get; private set; } = default!;

    public static Franchise Create(string franchiseName)
    {
        return new Franchise(franchiseName);
    }

    public void Update(string franchiseName)
    {
        FranchiseName = franchiseName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }

}
