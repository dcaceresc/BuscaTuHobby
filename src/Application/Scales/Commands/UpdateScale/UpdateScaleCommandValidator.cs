using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Scales.Commands.UpdateScale
{
    public class UpdateScaleCommandValidator : AbstractValidator<UpdateScaleCommand>
    {
        public UpdateScaleCommandValidator()
        {
            RuleFor(v => v.Id).NotNull().NotEmpty();
            RuleFor(v => v.Name).NotNull().NotEmpty();
        }
    }
}
