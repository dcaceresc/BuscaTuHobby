namespace Application.Dashboard.Queries.SearchProducts;

public class SearchProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public string ManufacturerName { get; set; } = default!;
    public string FranchiseName { get; set; } = default!;
    public int ProductViewCount { get; set; }
    public int StoreCount { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
    public int BestDiscount { get; set; }
    public List<string> Categories { get; set; } = default!;
}
