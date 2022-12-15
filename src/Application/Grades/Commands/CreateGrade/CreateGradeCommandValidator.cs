using FluentValidation;

namespace Application.Grades.Commands.CreateGrade;

public class CreateGradeCommandValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
        RuleFor(v => v.acronym).NotNull().NotEmpty();
    }
}

