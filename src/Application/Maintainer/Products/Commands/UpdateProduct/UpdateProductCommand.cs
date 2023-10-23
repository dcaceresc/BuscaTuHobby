using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Products.Commands.UpdateProduct;

public class UpdateProductCommand : IRequest
{
    public int id { get; set; }
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

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
    {
        private readonly IApplicationDbContext _context;

        public UpdateProductCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Products.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Product), request.id);

            entity.name = request.name;
            entity.scaleId = request.scaleId;
            entity.manufacturerId = request.manufacturerId;
            entity.franchiseId = request.franchiseId;
            entity.serieId = request.serieId;
            entity.hasBase = request.hasBase;
            entity.targetAge = request.targetAge;
            entity.size = request.size;
            entity.description = request.description;
            entity.releaseDate = request.releaseDate;

            await _context.SaveChangesAsync(cancellationToken);

            var productCategoriesOld = await _context.ProductCategories.Where(x => x.productId == request.id).ToListAsync(cancellationToken);

            _context.ProductCategories.RemoveRange(productCategoriesOld);

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

        }
    }
}
