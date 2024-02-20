namespace Domain.Entities;

public class Manufacturer : AuditableEntity
{
    private Manufacturer(string manufacturerName)
    {
        ManufacturerId = Guid.NewGuid();
        ManufacturerName = manufacturerName;
        IsActive = true;
    }

    public Guid ManufacturerId { get; private set; }
    public string ManufacturerName { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public virtual ICollection<Product> Products { get; private set; } = default!;

    public static Manufacturer Create(string manufacturerName)
    {
        return new Manufacturer(manufacturerName);
    }

    public void Update(string manufacturerName)
    {
        ManufacturerName = manufacturerName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
