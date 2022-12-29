using FluentValidation;

namespace Application.Grades.Commands.UpdateGrade;

public class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
{
    public UpdateGradeCommandValidator()
    {
        RuleFor(v => v.id).NotNull().NotEmpty();
        RuleFor(v => v.name).NotNull().NotEmpty();
        RuleFor(v => v.acronym).NotNull().NotEmpty();
    }
}

