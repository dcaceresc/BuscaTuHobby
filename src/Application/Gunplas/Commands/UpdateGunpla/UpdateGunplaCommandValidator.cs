
using FluentValidation;

namespace Application.Gunplas.Commands.UpdateGunpla;

public class UpdateGunplaCommandValidator : AbstractValidator<UpdateGunplaCommand>
{
    public UpdateGunplaCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.name).NotNull();
        RuleFor(v => v.gradeId).NotNull();
        RuleFor(v => v.scaleId).NotNull();
        RuleFor(v => v.manufacturerId).NotNull();
        RuleFor(v => v.serieId).NotNull();
        RuleFor(v => v.hasBase).NotNull();
        RuleFor(v => v.description).NotNull();
        RuleFor(v => v.releaseDate).NotNull();
    }
}