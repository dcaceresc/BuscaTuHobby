using FluentValidation;

namespace Application.Maintainer.Manufacturers.Commands.CreateManufacturer;

public class CreateManufacturerValidator : AbstractValidator<CreateManufacturer>
{
    public CreateManufacturerValidator()
    {
        RuleFor(v => v.ManufacturerName).NotNull().NotEmpty();
    }
}

