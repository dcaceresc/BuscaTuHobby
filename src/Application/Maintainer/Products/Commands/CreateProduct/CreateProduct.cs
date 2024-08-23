﻿namespace Application.Maintainer.Products.Commands.CreateProduct;

public record CreateProduct : IRequest<ApiResponse>
{
    public string ProductName { get; init; } = default!;
    public Guid ScaleId { get; init; }
    public Guid ManufacturerId { get; init; }
    public Guid FranchiseId { get; init; }
    public Guid SerieId { get; init; }
    public bool ProductHasBase { get; init; }
    public string ProductTargetAge { get; init; } = default!;
    public string ProductSize { get; init; } = default!;
    public string ProductDescription { get; init; } = default!;
    public DateTime ProductReleaseDate { get; init; }
    public IList<Guid> CategoryIds { get; init; } = default!;

}

public class CreateProductHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateProduct, ApiResponse>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        try
        {
            var product = Product.Create(request.ProductName, request.ScaleId, request.ManufacturerId, request.FranchiseId, request.SerieId, request.ProductHasBase, request.ProductTargetAge, request.ProductSize, request.ProductDescription, request.ProductReleaseDate);

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var item in request.CategoryIds)
            {
                var categoryProduct = product.AssignCategory(item);

                _context.ProductCategories.Add(categoryProduct);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success("Producto creado correctamente");
        }
        catch (Exception)
        {
            return _responseService.Fail("No se pudo crear el producto");
        }
    }
}