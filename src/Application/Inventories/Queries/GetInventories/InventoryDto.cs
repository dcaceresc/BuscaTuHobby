using Domain.Entities;

namespace Application.Inventories.Queries.GetInventories;

public class InventoryDto
{
    public int id { get; set; }
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Inventory, InventoryDto>();
        }
    }
}