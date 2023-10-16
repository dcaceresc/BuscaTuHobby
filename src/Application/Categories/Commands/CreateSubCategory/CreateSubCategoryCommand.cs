using Domain.Entities;

namespace Application.Categories.Commands.CreateSubCategory;

public class CreateSubCategoryCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public int categoryId { get; set; }
}

public class CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new SubCategory
        {
            name = request.name,
            categoryId = request.categoryId,
            active = true
        };

        _context.SubCategories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.id;
    }
}
