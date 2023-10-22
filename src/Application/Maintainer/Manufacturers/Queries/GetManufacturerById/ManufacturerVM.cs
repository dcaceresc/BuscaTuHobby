using Domain.Entities;

namespace Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
public class ManufacturerVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Manufacturer, ManufacturerVM>();
        }
    }
}
