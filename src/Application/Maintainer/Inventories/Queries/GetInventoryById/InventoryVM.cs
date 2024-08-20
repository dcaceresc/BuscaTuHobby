namespace Application.Maintainer.Inventories.Queries.GetInventoryById;
public class InventoryVM
{
    public Guid InventoryId { get; set; }
    public Guid StoreId { get; set; }
    public Guid ProductId { get; set; }
    public int Price { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Inventory, InventoryVM>();
        }
    }
}
