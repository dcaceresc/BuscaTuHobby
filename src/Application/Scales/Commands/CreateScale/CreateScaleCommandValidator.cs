using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Scales.Commands.CreateScale
{
    public class CreateScaleCommandValidator : AbstractValidator<CreateScaleCommand>
    {
        public CreateScaleCommandValidator()
        {
            RuleFor(v => v.Name).NotNull().NotEmpty();
        }
    }
}
