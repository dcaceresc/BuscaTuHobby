using Domain.Entities;

namespace Application.Maintainer.Products.Commands.CreateProduct;

public class CreateProductCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public int scaleId { get; set; }
    public int manufacturerId { get; set; }
    public int franchiseId { get; set; }
    public int? serieId { get; set; }
    public bool hasBase { get; set; }
    public string targetAge { get; set; } = default!;
    public string size { get; set; } = default!;
    public string description { get; set; } = default!;
    public DateTime releaseDate { get; set; }
    public IList<int> categories { get; set; } = default!;

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
                franchiseId = request.franchiseId,
                serieId = request.serieId,
                hasBase = request.hasBase,
                targetAge = request.targetAge,
                size = request.size,
                description = request.description,
                releaseDate = request.releaseDate,
                active = true
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);


            foreach (var item in request.categories)
            {
                var categoryProduct = new ProductCategory
                {
                    categoryId = item,
                    productId = entity.id
                };

               _context.ProductCategories.Add(categoryProduct);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }


}

