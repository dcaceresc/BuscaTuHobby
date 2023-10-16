using FluentValidation;

namespace Application.Series.Commands.UpdateSerie;

public class UpdateSerieCommandValidator : AbstractValidator<UpdateSerieCommand>
{
    public UpdateSerieCommandValidator()
    {
        RuleFor(v => v.name).NotNull().NotEmpty();
    }
}

