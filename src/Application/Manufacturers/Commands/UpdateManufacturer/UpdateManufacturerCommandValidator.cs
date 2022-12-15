using FluentValidation;

namespace Application.Manufacturers.Commands.UpdateManufacturer;

public class UpdateManufacturerCommandValidator : AbstractValidator<UpdateManufacturerCommand>
{
    public UpdateManufacturerCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();

    }
}

