namespace Domain.Entities;

public class PostType : AuditableEntity
{
    private PostType(string postTypeName)
    {
        PostTypeId = Guid.NewGuid();
        PostTypeName = postTypeName;
        IsActive = true;
    }

    public Guid PostTypeId { get; private set; }
    public string PostTypeName { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public virtual ICollection<Post> Posts { get; private set; } = new List<Post>();

    public static PostType Create(string postTypeName)
    {
        return new PostType(postTypeName);
    }

    public void Update(string postTypeName)
    {
        PostTypeName = postTypeName;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
