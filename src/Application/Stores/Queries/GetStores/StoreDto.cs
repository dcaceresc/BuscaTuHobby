using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Stores.Queries.GetStores;

public class StoreDto : IMapFrom<StoreDto>
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string address { get; set; } = default!;
    public int ranking { get; set; }
    public bool active { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Store, StoreDto>();
    }
}