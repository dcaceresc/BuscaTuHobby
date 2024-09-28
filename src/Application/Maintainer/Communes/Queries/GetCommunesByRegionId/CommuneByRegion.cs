namespace Application.Maintainer.Communes.Queries.GetCommunesByRegionId;
public class CommuneByRegion
{
    public Guid CommuneId { get; set; }
    public string CommuneName { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Commune, CommuneByRegion>();
        }
    }
}
