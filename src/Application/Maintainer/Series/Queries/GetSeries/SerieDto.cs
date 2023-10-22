using Domain.Entities;

namespace Application.Maintainer.Series.Queries.GetSeries;

public class SerieDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieDto>();
        }
    }

}

