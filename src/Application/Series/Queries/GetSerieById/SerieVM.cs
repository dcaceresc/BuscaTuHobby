using Domain.Entities;

namespace Application.Series.Queries.GetSerieById;

public class SerieVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public int universeId { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieVM>();
        }
    }
}