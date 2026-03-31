namespace Application.Dashboard.Queries.GetRecentPosts;

public class RecentPostDto
{
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = default!;
    public string PostContent { get; set; } = default!;
    public string PostTypeName { get; set; } = default!;
    public int PostViewCount { get; set; }
}
