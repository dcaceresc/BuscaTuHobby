namespace Application.Dashboard.Queries.GetFeaturedStores;

public class FeaturedStoreDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreIcon { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;
    public int ProductCount { get; set; }
    public int OfferCount { get; set; }
}
