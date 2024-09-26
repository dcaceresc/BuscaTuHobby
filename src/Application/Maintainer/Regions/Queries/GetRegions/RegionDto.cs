namespace Application.Maintainer.Regions.Queries.GetRegions;

public class RegionDto
{
    public Guid RegionId { get; set; }
    public string RegionName { get; set; } = default!;
    public int RegionOrder { get; set; }
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Region, RegionDto>();
        }
    }
}