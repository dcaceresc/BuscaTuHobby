using FluentValidation;

namespace Application.Maintainer.Series.Commands.UpdateSerie;

public class UpdateSerieCommandValidator : AbstractValidator<UpdateSerieCommand>
{
    public UpdateSerieCommandValidator()
    {
        RuleFor(v => v.SerieName).NotNull().NotEmpty();
    }
}

