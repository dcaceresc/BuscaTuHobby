using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Grades.Queries.GetGrades
{
    public class GradeVm : IMapFrom<Grade>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Acronym { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Grade, GradeVm>();
        }
    }
}
