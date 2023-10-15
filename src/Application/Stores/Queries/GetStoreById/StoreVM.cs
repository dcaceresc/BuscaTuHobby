using Domain.Entities;

namespace Application.Stores.Queries.GetStoreById;

public class StoreVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string address { get; set; } = default!;
    public string webSite { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Store,StoreVM>();
        }
    }
}
