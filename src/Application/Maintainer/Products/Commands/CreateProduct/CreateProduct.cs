namespace Application.Maintainer.Products.Commands.CreateProduct;

public record CreateProduct : IRequest<ApiResponse<Guid>>
{
    public string ProductName { get; init; } = default!;
    public Guid ManufacturerId { get; init; }
    public Guid FranchiseId { get; init; }
    public Guid? SerieId { get; init; }
    public bool ProductHasBase { get; init; }
    public string ProductTargetAge { get; init; } = default!;
    public string ProductSize { get; init; } = default!;
    public string ProductDescription { get; init; } = default!;
    public DateOnly ProductReleaseDate { get; init; }
    public IList<Guid> CategoryIds { get; init; } = default!;

    // Grado, escala y número de parte del producto. El grado viaja como entero (0 sin grado ... 6 otro) y la
    // escala y el número de parte son opcionales.
    public int Grade { get; init; }
    public string? Scale { get; init; }
    public string? PartNumber { get; init; }

}

public class CreateProductHandler(IApplicationDbContext context, IApiResponseService responseService) : IRequestHandler<CreateProduct, ApiResponse<Guid>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IApiResponseService _responseService = responseService;

    public async Task<ApiResponse<Guid>> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        try
        {
            // Se valida antes de crear para que el rechazo tenga el mensaje exacto en español y no se guarde nada.
            if (!GunplaGradeRules.IsValidGrade(request.Grade))
                return _responseService.Fail<Guid>(GunplaGradeRules.InvalidGradeMessage);
            if (!GunplaGradeRules.IsValidScale(request.Scale))
                return _responseService.Fail<Guid>(GunplaGradeRules.InvalidScaleMessage);
            if (!GunplaGradeRules.IsValidPartNumber(request.PartNumber))
                return _responseService.Fail<Guid>(GunplaGradeRules.InvalidPartNumberMessage);

            var product = Product.Create(request.ProductName, request.ManufacturerId, request.FranchiseId, request.SerieId, request.ProductHasBase, request.ProductTargetAge, request.ProductSize, request.ProductDescription, request.ProductReleaseDate, (GunplaGrade)request.Grade, request.Scale, request.PartNumber);

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var item in request.CategoryIds)
            {
                var categoryProduct = product.AssignCategory(item);

                _context.ProductCategories.Add(categoryProduct);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _responseService.Success(product.ProductId);
        }
        catch (Exception)
        {
            return _responseService.Fail<Guid>("No se pudo crear el producto");
        }
    }
}
