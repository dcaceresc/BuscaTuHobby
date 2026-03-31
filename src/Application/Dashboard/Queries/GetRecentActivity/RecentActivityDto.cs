namespace Application.Dashboard.Queries.GetRecentActivity;

public class RecentActivityDto
{
    public string ActivityType { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
