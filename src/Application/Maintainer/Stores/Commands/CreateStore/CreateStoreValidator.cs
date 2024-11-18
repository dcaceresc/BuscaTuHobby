using FluentValidation;

namespace Application.Maintainer.Stores.Commands.CreateStore;

public class CreateStoreValidator : AbstractValidator<CreateStore>
{
    public CreateStoreValidator()
    {
        RuleFor(v => v.StoreName).NotNull();
        RuleFor(v => v.StoreWebSite).NotNull();
    }
}
