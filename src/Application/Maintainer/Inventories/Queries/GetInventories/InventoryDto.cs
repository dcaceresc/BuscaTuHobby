namespace Application.Maintainer.Inventories.Queries.GetInventories;

public class InventoryDto
{
    public Guid InventoryId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StoreName { get; set; } = default!;
    public string Price { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Inventory, InventoryDto>()
                .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.ProductName))
                .ForMember(d => d.StoreName, opt => opt.MapFrom(s => s.Store.StoreName))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price.ToString("C0")));
        }
    }
}