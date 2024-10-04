namespace Application.Maintainer.Products.Commands.UpdateProduct;

public class UpdateProduct : IRequest<ApiResponse>
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public Guid ManufacturerId { get; init; }
    public Guid FranchiseId { get; init; }
    public Guid SerieId { get; init; }
    public bool ProductHasBase { get; init; }
    public string ProductTargetAge { get; init; } = default!;
    public string ProductSize { get; init; } = default!;
    public string ProductDescription { get; init; } = default!;
    public DateOnly ProductReleaseDate { get; init; }
    public IList<Guid> CategoryIds { get; init; } = default!;
}

public class UpdateProductHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<UpdateProduct, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(UpdateProduct request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

            Guard.Against.NotFound(product, $"No existe Producto con la Id {request.ProductId}");

            product.Update(request.ProductName, request.ManufacturerId, request.FranchiseId, request.SerieId, request.ProductHasBase, request.ProductTargetAge, request.ProductSize, request.ProductDescription, request.ProductReleaseDate);

            var productCategoriesOld = await _context.ProductCategories.Where(x => x.ProductId == request.ProductId).ToListAsync(cancellationToken);

            foreach (var item in request.CategoryIds)
            {
                var existingProductCategory = productCategoriesOld.FirstOrDefault(x => x.CategoryId == item);

                if (existingProductCategory == null)
                {
                    var categoryProduct = ProductCategory.Create(item, request.ProductId);

                    _context.ProductCategories.Add(categoryProduct);
                }
                else
                {
                    existingProductCategory.LastModified = DateTime.Now;
                }

            }

            foreach (var item in productCategoriesOld)
            {
                if (!request.CategoryIds.Contains(item.CategoryId))
                {
                    _context.ProductCategories.Remove(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Producto actualizado correctamente");
        }
        catch (Common.Exceptions.NotFoundException e)
        {
            return _responseService.Fail(e.Message);
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo actualizar el producto");
        }
    }
}
