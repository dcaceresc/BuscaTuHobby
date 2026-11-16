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

    // Grado, escala y número de parte del producto. El grado viaja como entero (0 sin grado ... 6 otro) y la
    // escala y el número de parte son opcionales. Llegan sin valor por defecto a propósito: un valor omitido en la
    // actualización borraría en silencio lo que cargó el administrador.
    public int Grade { get; init; }
    public string? Scale { get; init; }
    public string? PartNumber { get; init; }
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

            // Se valida antes de mutar para que el rechazo tenga el mensaje exacto en español y no cambie nada.
            if (!GunplaGradeRules.IsValidGrade(request.Grade))
                return _responseService.Fail(GunplaGradeRules.InvalidGradeMessage);
            if (!GunplaGradeRules.IsValidScale(request.Scale))
                return _responseService.Fail(GunplaGradeRules.InvalidScaleMessage);
            if (!GunplaGradeRules.IsValidPartNumber(request.PartNumber))
                return _responseService.Fail(GunplaGradeRules.InvalidPartNumberMessage);

            product.Update(request.ProductName, request.ManufacturerId, request.FranchiseId, request.SerieId, request.ProductHasBase, request.ProductTargetAge, request.ProductSize, request.ProductDescription, request.ProductReleaseDate, (GunplaGrade)request.Grade, request.Scale, request.PartNumber);

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
