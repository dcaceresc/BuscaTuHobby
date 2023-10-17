using Domain.Entities;

namespace Application.Manufacturers.Queries.GetManufacturers;

public class ManufacturerDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Manufacturer, ManufacturerDto>();
        }
    }
}