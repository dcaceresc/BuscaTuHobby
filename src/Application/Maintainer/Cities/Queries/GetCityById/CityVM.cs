namespace Application.Maintainer.Cities.Queries.GetCityById;

public class CityVM
{
    public Guid CityId { get; set; }
    public string CityName { get; set; } = default!;
    public Guid RegionId { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<City,CityVM>();
        }
    }
}