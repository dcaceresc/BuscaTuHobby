namespace Application.Maintainer.Posts.Queries.GetPostById;

public class PostVM
{
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = default!;
    public string PostContent { get; set; } = default!;
    public Guid PostTypeId { get; set; }
    public IList<Guid> CategoryIds { get; set; } = default!;
}
