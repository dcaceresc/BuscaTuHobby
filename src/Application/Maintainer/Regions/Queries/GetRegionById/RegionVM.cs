namespace Application.Maintainer.Regions.Queries.GetRegionById;

public class RegionVM
{
    public Guid RegionId { get; set; }
    public string RegionName { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Region, RegionVM>();
        }
    }
}