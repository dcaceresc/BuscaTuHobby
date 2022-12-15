using FluentValidation;

namespace Application.Photos.Commands.UpdatePhoto;
public class UpdatePhotoCommandValidator : AbstractValidator<UpdatePhotoCommand>
{
    public UpdatePhotoCommandValidator()
    {
        RuleFor(v => v.id).NotNull();
        RuleFor(v => v.order).NotNull();
        RuleFor(v => v.imageData).NotNull();
        RuleFor(v => v.gunplaId).NotNull();
    }
}