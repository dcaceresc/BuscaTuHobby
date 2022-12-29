using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Series.Queries.GetSeries;

public class SerieVm : IMapFrom<Serie>
{
    public int id { get; set; }
    public string name { get; set; }
    public int universeId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Serie, SerieVm>();
    }
}

