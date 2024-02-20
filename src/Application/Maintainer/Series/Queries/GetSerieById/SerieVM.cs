using Domain.Entities;

namespace Application.Maintainer.Series.Queries.GetSerieById;

public class SerieVM
{
    public Guid SerieId { get; set; }
    public string SerieName { get; set; } = default!;
    public Guid FranchiseId { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieVM>();
        }
    }
}