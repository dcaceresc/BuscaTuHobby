using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Gunplas.Commands.UpdateGunpla
{
    public class UpdateGunplaCommandValidator : AbstractValidator<UpdateGunplaCommand>
    {
        public UpdateGunplaCommandValidator()
        {
            RuleFor(v => v.Id).NotNull();
            RuleFor(v => v.Name).NotNull();
            RuleFor(v => v.GradeId).NotNull();
            RuleFor(v => v.ScaleId).NotNull();
            RuleFor(v => v.ManufacturerId).NotNull();
            RuleFor(v => v.SerieId).NotNull();
            RuleFor(v => v.Base).NotNull();
            RuleFor(v => v.Description).NotNull();
            RuleFor(v => v.ReleaseDate).NotNull();
        }
    }
}
