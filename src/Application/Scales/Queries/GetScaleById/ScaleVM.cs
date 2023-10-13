using Domain.Entities;

namespace Application.Scales.Queries.GetScaleById;

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
