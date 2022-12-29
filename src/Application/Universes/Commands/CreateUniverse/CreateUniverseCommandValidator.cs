using FluentValidation;

namespace Application.Universes.Commands.CreateUniverse;

public class CreateUniverseCommandValidator : AbstractValidator<CreateUniverseCommand>
{
    public CreateUniverseCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

