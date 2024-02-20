namespace Application.Maintainer.Categories.Commands.ToggleCategory;
public record ToggleCategoryCommand(Guid CategoryId) : IRequest;

public class ToggleSubCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleCategoryCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

        Guard.Against.NotFound(request.CategoryId, entity);

        entity.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);
    }
}