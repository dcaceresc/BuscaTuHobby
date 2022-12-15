using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Universes.Queries.GetUniverses;

public class UniverseVm : IMapFrom<Universe>
{
    public int id { get; set; }
    public string name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Universe, UniverseVm>();
    }

}

