namespace Application.Dashboard.Queries.GetMostSearchedProducts;

public class MostSearchedProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public string ProductDescription { get; set; } = default!;
    public int ProductViewCount { get; set; }
    public int StoreCount { get; set; }
    public List<string> StoreNames { get; set; } = default!;
}
