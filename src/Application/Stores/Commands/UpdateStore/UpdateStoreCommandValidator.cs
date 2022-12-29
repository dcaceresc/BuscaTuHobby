using FluentValidation;

namespace Application.Stores.Commands.UpdateStore;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStoreCommand>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.name).NotNull();
        RuleFor(v => v.address).NotNull();
        RuleFor(v => v.ranking).NotNull();
    }
}
