using Domain.Entities;

namespace Application.Maintainer.Series.Queries.GetSeriesByFranchise;

public class SerieByFranchiseDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie,SerieByFranchiseDto>();
        }
    }
}