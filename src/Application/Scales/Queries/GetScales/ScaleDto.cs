using Application.Common.Mappings;
using Domain.Entities;

namespace Application.Scales.Queries.GetScales;

public class ScaleDto : IMapFrom<Scale>
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public bool active { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Scale, ScaleDto>();
    }
}

