using Domain.Entities;

namespace Application.Photos.Commands.CreatePhoto;

public class CreatePhotoCommand : IRequest<int>
{
    public int order { get; set; }
    public byte[] imageData { get; set; } = default!;
    public int productId { get; set; }

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
                productId = request.productId
            };

            _context.Photos.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }
}

