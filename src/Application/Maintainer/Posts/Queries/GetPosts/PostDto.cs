namespace Application.Maintainer.Posts.Queries.GetPosts;

public class PostDto
{
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = default!;
    public string PostTypeName { get; set; } = default!;
    public IList<string> Categories { get; set; } = default!;
    public bool IsActive { get; set; }
}
