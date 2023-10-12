using AutoMapper;
using Domain.Entities;

namespace Application.Universes.Queries.GetUniverseById;

public class UniverseVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Universe,UniverseVM>();
        }
    }
}