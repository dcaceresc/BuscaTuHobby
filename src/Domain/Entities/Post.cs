namespace Domain.Entities;

public class Post : AuditableEntity
{
    private Post(string postTitle, string postContent, Guid postTypeId)
    {
        PostId = Guid.NewGuid();
        PostTitle = postTitle;
        PostContent = postContent;
        PostTypeId = postTypeId;
        PostViewCount = 0;
        IsActive = true;
    }

    public Guid PostId { get; private set; }
    public string PostTitle { get; private set; } = default!;
    public string PostContent { get; private set; } = default!;
    public Guid PostTypeId { get; private set; }
    public int PostViewCount { get; private set; }
    public bool IsActive { get; private set; }

    public virtual PostType PostType { get; private set; } = default!;
    public virtual ICollection<PostCategory> PostCategories { get; private set; } = new List<PostCategory>();

    public static Post Create(string postTitle, string postContent, Guid postTypeId)
    {
        return new Post(postTitle, postContent, postTypeId);
    }

    public void Update(string postTitle, string postContent, Guid postTypeId)
    {
        PostTitle = postTitle;
        PostContent = postContent;
        PostTypeId = postTypeId;
    }

    public PostCategory AssignCategory(Guid categoryId)
    {
        var postCategory = PostCategory.Create(PostId, categoryId);
        return postCategory;
    }

    public void IncrementViewCount()
    {
        PostViewCount++;
    }

    public void SetViewCount(int count)
    {
        PostViewCount = count;
    }

    public void ToggleActive()
    {
        IsActive = !IsActive;
    }
}
