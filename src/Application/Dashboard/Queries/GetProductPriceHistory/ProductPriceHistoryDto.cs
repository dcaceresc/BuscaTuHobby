namespace Application.Dashboard.Queries.GetProductPriceHistory;

public class ProductPriceHistoryDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public bool HasHistory { get; set; }
    public List<PricePointDto> Points { get; set; } = [];
}

public class PricePointDto
{
    public DateTime Date { get; set; }
    public int MinPrice { get; set; }
    public int MaxPrice { get; set; }
    public int OfferCount { get; set; }
}
