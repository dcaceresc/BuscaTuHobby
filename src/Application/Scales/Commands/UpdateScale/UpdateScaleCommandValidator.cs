using FluentValidation;

namespace Application.Scales.Commands.UpdateScale;

public class UpdateScaleCommandValidator : AbstractValidator<UpdateScaleCommand>
{
    public UpdateScaleCommandValidator()
    {
        RuleFor(v => v.id).NotNull().NotEmpty();
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}