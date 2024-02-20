using FluentValidation;

namespace Application.Maintainer.Scales.Commands.CreateScale;

public class CreateScaleCommandValidator : AbstractValidator<CreateScaleCommand>
{
    public CreateScaleCommandValidator()
    {
        RuleFor(v => v.ScaleName).NotNull().NotEmpty();
    }
}
