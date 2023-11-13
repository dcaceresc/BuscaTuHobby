using Domain.Entities;

namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public class InventoryVM 
{
    public int id { get; set; }
    public int storeId { get; set; }
    public int productId { get; set; }
    public int price { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Inventory, InventoryVM>();
        }
    }
}
