namespace Application.Maintainer.Stores.Queries.GetStoreById;

public class StoreVM
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;
    public string StoreIcon { get; set; } = default!;
    public int StoreOrder { get; set; }
    public string StoreSlug { get; set; } = default!;
    public List<StoreAddressDto> StoreAddress { get; set; } = default!;
}

public class StoreAddressDto
{
    public Guid StoreAddressId { get; set; }
    public string Street { get; set; } = default!;
    public Guid CommuneId { get; set; }
    public Guid RegionId { get; set; }
    public string? ZipCode { get; set; }
    
}
