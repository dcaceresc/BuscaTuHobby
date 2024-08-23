using FluentValidation;

namespace Application.Maintainer.Scales.Commands.UpdateScale;

public class UpdateScaleCommandValidator : AbstractValidator<UpdateScale>
{
    public UpdateScaleCommandValidator()
    {
        RuleFor(v => v.ScaleId).NotNull().NotEmpty();
        RuleFor(v => v.ScaleName).NotNull().NotEmpty();
    }
}