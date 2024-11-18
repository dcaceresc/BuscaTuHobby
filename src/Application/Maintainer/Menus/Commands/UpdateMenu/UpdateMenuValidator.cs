using FluentValidation;

namespace Application.Maintainer.Menus.Commands.UpdateMenu;

public class UpdateMenuValidator : AbstractValidator<UpdateMenu>
{
    public UpdateMenuValidator()
    {
        RuleFor(v => v.MenuId).NotNull().NotEmpty();
        RuleFor(v => v.MenuName).NotNull().NotEmpty();
    }
}

