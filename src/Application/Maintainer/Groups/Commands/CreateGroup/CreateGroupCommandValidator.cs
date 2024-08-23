using FluentValidation;

namespace Application.Maintainer.Groups.Commands.CreateGroup;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroup>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(v => v.GroupName).NotNull().NotEmpty();
    }
}

