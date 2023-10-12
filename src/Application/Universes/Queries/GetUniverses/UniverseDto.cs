using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Universes.Queries.GetUniverses;

public class UniverseDto
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;
    public bool active { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Universe, UniverseDto>();
        }
    }

}

