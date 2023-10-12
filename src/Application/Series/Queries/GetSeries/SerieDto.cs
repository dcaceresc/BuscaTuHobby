using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Series.Queries.GetSeries;

public class SerieDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string universe { get; set; } = default!;
    public bool active { get; set; }


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Serie, SerieDto>()
                .ForMember(d => d.universe, opt => opt.MapFrom(s => s.Universe.name));
        }
    }

}

