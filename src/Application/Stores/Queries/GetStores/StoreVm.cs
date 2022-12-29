using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Stores.Queries.GetStores;

public class StoreVm : IMapFrom<StoreVm>
{
    public int id { get; set; }
    public string name { get; set; }
    public string address { get; set; }
    public int ranking { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Store, StoreVm>();
    }
}