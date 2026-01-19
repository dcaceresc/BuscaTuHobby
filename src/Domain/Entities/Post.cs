namespace Domain.Entities;

public class Post: AuditableEntity
{
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = default!;
    public string PostContent { get; set; } = default!;
    public Guid PostTypeId { get; set; }
    public PostType PostType {get;set;} = default!;
    public bool IsActive { get; set; }

    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
}