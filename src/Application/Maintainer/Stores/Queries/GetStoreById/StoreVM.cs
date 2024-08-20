namespace Application.Maintainer.Stores.Queries.GetStoreById;

public class StoreVM
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = default!;
    public string StoreAddress { get; set; } = default!;
    public string StoreWebSite { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Store, StoreVM>();
        }
    }
}
