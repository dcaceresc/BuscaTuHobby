namespace Application.Maintainer.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid CategoryId, string CategoryName, Guid GroupId) : IRequest;

public class UpdateSubCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FindAsync([request.CategoryId], cancellationToken);

        Guard.Against.NotFound(request.CategoryId, entity);

        entity.Update(request.CategoryName, request.GroupId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
