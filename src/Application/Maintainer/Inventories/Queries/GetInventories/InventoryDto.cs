using Domain.Entities;

namespace Application.Maintainer.Inventories.Queries.GetInventories;

public class InventoryDto
{
    public int id { get; set; }
    public string productName { get; set; } = default!;
    public string storeName { get; set; } = default!;
    public int price { get; set; }
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Inventory, InventoryDto>()
                .ForMember(d => d.productName, opt => opt.MapFrom(s => s.Product.name))
                .ForMember(d => d.storeName, opt => opt.MapFrom(s => s.Store.name));
        }
    }
}