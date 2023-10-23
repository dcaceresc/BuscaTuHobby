using Domain.Entities;

namespace Application.Maintainer.Series.Queries.GetSeries;

public class SerieDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string franchiseName { get; set; } = default!;
    public bool active { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieDto>().
                ForMember(d => d.franchiseName, opt => opt.MapFrom(x => x.Franchise.name));
        }
    }

}

