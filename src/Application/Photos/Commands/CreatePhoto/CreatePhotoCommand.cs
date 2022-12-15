using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Photos.Commands.CreatePhoto;

public class CreatePhotoCommand : IRequest<int>
{
    public int order { get; set; }
    public byte[] imageData { get; set; }
    public int gunplaId { get; set; }

    public class CreatePhotoCommandHandler : IRequestHandler<CreatePhotoCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreatePhotoCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreatePhotoCommand request, CancellationToken cancellationToken)
        {
            var entity = new Photo
            {
                order = request.order,
                imageData = request.imageData,
                gunplaId = request.gunplaId
            };

            _context.Photos.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}

