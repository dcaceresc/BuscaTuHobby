using FluentValidation;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(v => v.name).NotNull();
        RuleFor(v => v.address).NotNull();
        RuleFor(v => v.webSite).NotNull();
    }
}
