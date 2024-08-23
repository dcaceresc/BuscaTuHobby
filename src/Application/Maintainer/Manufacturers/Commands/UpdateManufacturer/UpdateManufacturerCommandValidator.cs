using FluentValidation;

namespace Application.Maintainer.Manufacturers.Commands.UpdateManufacturer;

public class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturer>
{
    public UpdateManufacturerCommandValidator()
    {
        RuleFor(v => v.ManufacturerName).NotNull().NotEmpty();

    }
}

