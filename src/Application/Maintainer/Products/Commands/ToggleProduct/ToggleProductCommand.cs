namespace Application.Maintainer.Products.Commands.ToggleProduct;

public record ToggleProductCommand(Guid ProductId) : IRequest;

public class ToggleProductCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleProductCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync([request.ProductId], cancellationToken);

        Guard.Against.NotFound(request.ProductId, product);

        product.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}