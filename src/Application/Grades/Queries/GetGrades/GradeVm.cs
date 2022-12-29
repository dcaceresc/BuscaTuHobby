using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Grades.Queries.GetGrades;

public class GradeVm : IMapFrom<Grade>
{
    public int id { get; set; }
    public string name { get; set; }
    public string acronym { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Grade, GradeVm>();
    }
}

