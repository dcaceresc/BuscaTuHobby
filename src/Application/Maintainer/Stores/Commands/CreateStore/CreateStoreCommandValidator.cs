using FluentValidation;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public class CreateStoreCommandValidator : AbstractValidator<CreateStore>
{
    public CreateStoreCommandValidator()
    {
        RuleFor(v => v.StoreName).NotNull();
        RuleFor(v => v.StoreAddress).NotNull();
        RuleFor(v => v.StoreWebSite).NotNull();
    }
}
