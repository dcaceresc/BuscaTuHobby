using FluentValidation;

namespace Application.Scales.Commands.CreateScale;

public class CreateScaleCommandValidator : AbstractValidator<CreateScaleCommand>
{
    public CreateScaleCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}
