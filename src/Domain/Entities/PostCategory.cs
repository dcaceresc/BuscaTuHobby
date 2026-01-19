namespace Domain.Entities;

public class PostCategory : AuditableEntity
{
    public Guid PostId { get; set; }
    public Post Post { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
