using FluentValidation;

namespace Application.Universes.Commands.UpdateUniverse;

public class UpdateUniverseCommandValidator : AbstractValidator<UpdateUniverseCommand>
{
    public UpdateUniverseCommandValidator()
    {
        RuleFor(v => v.id).NotNull().NotEmpty();
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

