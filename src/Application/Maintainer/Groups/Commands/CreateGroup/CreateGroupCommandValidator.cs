using FluentValidation;

namespace Application.Maintainer.Groups.Commands.CreateGroup;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(v => v.GroupName).NotNull().NotEmpty();
    }
}

