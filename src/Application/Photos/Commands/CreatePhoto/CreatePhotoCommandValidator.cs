using FluentValidation;

namespace Application.Photos.Commands.CreatePhoto;
public class CreatePhotoCommandValidator : AbstractValidator<CreatePhotoCommand>
{
    public CreatePhotoCommandValidator()
    {
        RuleFor(v => v.order).NotNull();
        RuleFor(v => v.imageData).NotNull();
        RuleFor(v => v.gunplaId).NotNull();
    }
}