using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Categories.Commands.ToggleSubCategory;
public class ToggleSubCategoryCommand : IRequest
{
    public int id { get; set; }
}

public class ToggleSubCategoryCommandHandler : IRequestHandler<ToggleSubCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public ToggleSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ToggleSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SubCategories.FindAsync(request.id);

        if (entity == null)
            throw new NotFoundException(nameof(SubCategory), request.id);

        entity.active = !entity.active;

        await _context.SaveChangesAsync(cancellationToken);
    }
}