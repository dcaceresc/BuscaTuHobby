using FluentValidation;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStoreCommand>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(v => v.StoreName).NotNull();
        RuleFor(v => v.StoreAddress).NotNull();
        RuleFor(v => v.StoreWebSite).NotNull();
    }
}
