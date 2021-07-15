using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Grades.Commands.UpdateGrade
{
    public class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
    {
        public UpdateGradeCommandValidator()
        {
            RuleFor(v => v.Id).NotNull().NotEmpty();
            RuleFor(v => v.Name).NotNull().NotEmpty();
            RuleFor(v => v.Acronym).NotNull().NotEmpty();
        }
    }
}
