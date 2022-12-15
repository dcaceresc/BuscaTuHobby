using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Scales.Queries.GetScales;

public class ScaleVm : IMapFrom<Scale>
{
    public int id { get; set; }
    public string name { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Scale, ScaleVm>();
    }
}

