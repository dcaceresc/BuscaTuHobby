using FluentValidation;

namespace Application.Maintainer.Scales.Commands.CreateScale;

public class CreateScaleCommandValidator : AbstractValidator<CreateScaleCommand>
{
    public CreateScaleCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}
