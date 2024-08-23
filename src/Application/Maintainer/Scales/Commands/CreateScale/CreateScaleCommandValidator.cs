using FluentValidation;

namespace Application.Maintainer.Scales.Commands.CreateScale;

public class CreateScaleCommandValidator : AbstractValidator<CreateScale>
{
    public CreateScaleCommandValidator()
    {
        RuleFor(v => v.ScaleName).NotNull().NotEmpty();
    }
}
