namespace Application.Maintainer.Scales.Queries.GetScaleById;

public class ScaleVM
{
    public Guid ScaleId { get; set; }
    public string ScaleName { get; set; } = default!;


    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Scale, ScaleVM>();
        }
    }
}
