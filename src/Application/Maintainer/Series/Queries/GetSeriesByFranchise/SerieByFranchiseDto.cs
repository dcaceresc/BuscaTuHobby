namespace Application.Maintainer.Series.Queries.GetSeriesByFranchise;

public class SerieByFranchiseDto
{
    public Guid SerieId { get; set; }
    public string SerieName { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieByFranchiseDto>();
        }
    }
}