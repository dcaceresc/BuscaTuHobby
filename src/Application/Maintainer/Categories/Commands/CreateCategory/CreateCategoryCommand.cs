using Domain.Entities;

namespace Application.Maintainer.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string CategoryName, Guid GroupId) : IRequest<Guid>;

public class CreateSubCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = Category.Create(request.CategoryName, request.GroupId);

        _context.Categories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.CategoryId;
    }
}
