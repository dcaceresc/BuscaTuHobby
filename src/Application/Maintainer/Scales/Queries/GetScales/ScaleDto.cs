namespace Application.Maintainer.Scales.Queries.GetScales;

public class ScaleDto
{
    public Guid ScaleId { get; set; }
    public string ScaleName { get; set; } = default!;
    public bool IsActive { get; set; }

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Scale, ScaleDto>();
        }
    }
}

