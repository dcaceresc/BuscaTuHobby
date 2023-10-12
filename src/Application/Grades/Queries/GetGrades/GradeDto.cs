using Domain.Entities;

namespace Application.Grades.Queries.GetGrades;

public class GradeDto 
{
    public int id { get; set; }
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;

    public class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Grade, GradeDto>();
        }
    }
}

