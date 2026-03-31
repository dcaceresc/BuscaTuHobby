namespace Domain.Entities;

public class PostCategory : AuditableEntity
{
    private PostCategory(Guid postId, Guid categoryId)
    {
        PostId = postId;
        CategoryId = categoryId;
    }

    public Guid PostId { get; private set; }
    public Guid CategoryId { get; private set; }

    public virtual Post Post { get; private set; } = null!;
    public virtual Category Category { get; private set; } = null!;

    public static PostCategory Create(Guid postId, Guid categoryId)
    {
        return new PostCategory(postId, categoryId);
    }
}
