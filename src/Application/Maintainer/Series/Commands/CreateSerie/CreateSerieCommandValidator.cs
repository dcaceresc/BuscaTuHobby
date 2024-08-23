using FluentValidation;

namespace Application.Maintainer.Series.Commands.CreateSerie
{
    public class CreateSerieCommandValidator : AbstractValidator<CreateSerie>
    {
        public CreateSerieCommandValidator()
        {
            RuleFor(v => v.SerieName).NotNull().NotEmpty();
        }
    }
}
