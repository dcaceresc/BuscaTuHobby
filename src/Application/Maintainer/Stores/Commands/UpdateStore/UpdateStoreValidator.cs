using FluentValidation;

namespace Application.Maintainer.Stores.Commands.UpdateStore;

public class UpdateStoreValidator : AbstractValidator<UpdateStore>
{
    public UpdateStoreValidator()
    {
        RuleFor(v => v.StoreId).NotNull();
        RuleFor(v => v.StoreName).NotNull();
        RuleFor(v => v.StoreWebSite).NotNull();
    }
}
