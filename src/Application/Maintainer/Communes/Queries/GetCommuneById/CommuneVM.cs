namespace Application.Maintainer.Communes.Queries.GetCommuneById;

public class CommuneVM
{
    public Guid CommuneId { get; set; }
    public string CommuneName { get; set; } = default!;
    public Guid RegionId { get; set; }

}