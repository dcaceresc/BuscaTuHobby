namespace Application.Dashboard.Queries.GetBestDeals;

public class BestDealDto
{
    public Guid InventoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StoreName { get; set; } = default!;
    public int DiscountPercentage { get; set; }
    public int ProductViewCount { get; set; }
}
