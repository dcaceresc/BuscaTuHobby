using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Sales.Queries.GetSales;

public class SaleVm : IMapFrom<Sale>
{
    public int id { get; set; }
    public int gunplaId { get; set; }
    public int storeId { get; set; }
    public int price { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Sale, SaleVm>();
    }
}