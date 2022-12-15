using FluentValidation;

namespace Application.Manufacturers.Commands.CreateManufacturer;

public class CreateManufacturerCommandValidator : AbstractValidator<CreateManufacturerCommand>
{
    public CreateManufacturerCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

