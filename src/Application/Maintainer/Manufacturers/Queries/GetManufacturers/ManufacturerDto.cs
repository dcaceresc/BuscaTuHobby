namespace Application.Maintainer.Manufacturers.Queries.GetManufacturers;

public class ManufacturerDto
{
    public Guid ManufacturerId { get; set; }
    public string ManufacturerName { get; set; } = default!;
    public bool IsActive { get; set; }
    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Manufacturer, ManufacturerDto>();
        }
    }
}