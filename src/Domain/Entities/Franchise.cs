namespace Domain.Entities;
public class Franchise : AuditableEntity
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }

    public virtual ICollection<Serie> Serie { get; set; } = default!;
    public virtual ICollection<Product> Products { get; set; } = default!;
}
