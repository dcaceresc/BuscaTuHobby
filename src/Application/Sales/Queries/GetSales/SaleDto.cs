using Domain.Entities;

namespace Application.Sales.Queries.GetSales;

public class SaleDto
{
    public int id { get; set; }
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Sale, SaleDto>();
        }
    }
}