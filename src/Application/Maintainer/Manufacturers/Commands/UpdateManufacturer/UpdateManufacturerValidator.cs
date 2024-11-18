using FluentValidation;

namespace Application.Maintainer.Manufacturers.Commands.UpdateManufacturer;

public class UpdateManufacturerValidator : AbstractValidator<UpdateManufacturer>
{
    public UpdateManufacturerValidator()
    {
        RuleFor(v => v.ManufacturerName).NotNull().NotEmpty();

    }
}

