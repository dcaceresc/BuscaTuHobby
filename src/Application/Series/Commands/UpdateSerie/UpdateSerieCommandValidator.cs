using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Series.Commands.UpdateSerie
{
    public class UpdateSerieCommandValidator : AbstractValidator<UpdateSerieCommand>
    {
        public UpdateSerieCommandValidator()
        {
            RuleFor(v => v.Name).NotNull().NotEmpty();
            RuleFor(v => v.UniverseId).NotNull().NotEmpty();
        }
    }
}
