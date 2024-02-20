using FluentValidation;

namespace Application.Maintainer.Manufacturers.Commands.CreateManufacturer;

public class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
{
    public CreateManufacturerCommandValidator()
    {
        RuleFor(v => v.ManufacturerName).NotNull().NotEmpty();
    }
}

