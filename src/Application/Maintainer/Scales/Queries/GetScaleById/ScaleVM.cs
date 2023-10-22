using Domain.Entities;

namespace Application.Maintainer.Scales.Queries.GetScaleById;

public class ScaleVM
{
    public int id { get; set; }
    public string name { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Scale, ScaleVM>();
        }
    }
}
