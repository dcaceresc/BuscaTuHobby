using FluentValidation;

namespace Application.Maintainer.Stores.Commands.UpdateStore;

public class UpdateStoreCommandValidator : AbstractValidator<UpdateStore>
{
    public UpdateStoreCommandValidator()
    {
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.StoreName).NotNull();
        RuleFor(v => v.StoreAddress).NotNull();
        RuleFor(v => v.StoreWebSite).NotNull();
    }
}
