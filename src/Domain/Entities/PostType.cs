namespace Domain.Entities;

public class PostType: AuditableEntity
{
    public Guid PostTypeId { get; set; }
    public string PostTypeName { get; set; } = default!;
    public bool IsActive { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}