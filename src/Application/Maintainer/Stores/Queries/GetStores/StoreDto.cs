namespace Application.Maintainer.Stores.Queries.GetStores;

public class StoreDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;
    public string StoreIcon { get; set; } = default!;
    public int StoreOrder { get; set; }
    public string StoreSlug { get; set; } = default!;
    public List<string> StoreAddress { get; set; } = default!;
    public bool IsActive { get; set; }
}