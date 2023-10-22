using Domain.Entities;

namespace Application.Maintainer.Categories.Commands.CreateCategory;

public class CreateCategoryCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public int groupId { get; set; }
}

public class CreateSubCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = new Category
        {
            name = request.name,
            groupId = request.groupId,
            active = true
        };

        _context.Categories.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.id;
    }
}
