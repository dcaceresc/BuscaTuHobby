using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Manufacturers.Queries.GetManufacturers;

public class ManufacturerVm : IMapFrom<Manufacturer>
{
    public int id { get; set; }
    public string name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Manufacturer, ManufacturerVm>();
    }
}