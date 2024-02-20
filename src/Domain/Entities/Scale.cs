namespace Domain.Entities;

public class Scale : AuditableEntity
{
    private Scale(string scaleName)
    {
        ScaleId = Guid.NewGuid();
        ScaleName = scaleName;
        IsActive = true;
    }


    public Guid ScaleId { get; private set; }
    public string ScaleName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<Product> Products { get; private set; } = default!;


    public static Scale Create(string scaleName)
    {
        return new Scale(scaleName);
    }

    public void Update(string scaleName)
    {
        ScaleName = scaleName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
