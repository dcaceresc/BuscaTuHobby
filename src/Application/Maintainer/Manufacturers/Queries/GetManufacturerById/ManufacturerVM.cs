namespace Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
public class ManufacturerVM
{
    public Guid ManufacturerId { get; set; }
    public string ManufacturerName { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Manufacturer, ManufacturerVM>();
        }
    }
}
