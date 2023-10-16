using Domain.Entities;

namespace Application.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int serieId { get; set; }
    public bool hasBase { get; set; }
    public string description { get; set; } = default!;
    public DateTime releaseDate { get; set; }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateProductCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = new Product
            {
                name = request.name,
                scaleId = request.scaleId,
                manufacturerId = request.manufacturerId,
                serieId = request.serieId,
                hasBase = request.hasBase,
                description = request.description,
                releaseDate = request.releaseDate
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.id;
        }
    }


}

