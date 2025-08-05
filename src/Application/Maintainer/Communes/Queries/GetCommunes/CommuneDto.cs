namespace Application.Maintainer.Cities.Queries.GetCommunes;

public class CommuneDto
{
    public Guid CommuneId { get; set; }
    public string CommuneName { get; set; } = default!;
    public string RegionName { get; set; } = default!;
    public bool IsActive { get; set; }
}