namespace Application.Maintainer.Cities.Queries.GetCities;

public class CityDto
{
    public Guid CityId { get; set; }
    public string RegionName { get; set; } = default!;
    public bool IsActive { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<City, CityDto>()
                .ForMember(d => d.RegionName, opt => opt.MapFrom(s => s.Region.RegionName));
        }
    }
}