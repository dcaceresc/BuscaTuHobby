namespace Application.Maintainer.Regions.Queries.GetRegionById;

public class RegionVM
{
    public Guid RegionId { get; set; }
    public string RegionName { get; set; } = default!;
    public int RegionOrder { get; set; }
}