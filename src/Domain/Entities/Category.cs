namespace Domain.Entities;

public class Category : AuditableEntity
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int groupId { get; set; }
    public bool active { get; set; }
    public virtual Group Group { get; set; } = default!;
    public virtual ICollection<ProductCategory> CategoryProducts { get; set; } = default!;

}
