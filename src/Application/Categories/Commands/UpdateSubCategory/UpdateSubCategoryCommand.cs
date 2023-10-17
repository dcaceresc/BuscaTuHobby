using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Categories.Commands.UpdateSubCategory;

public class UpdateSubCategoryCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
}

public class UpdateSubCategoryCommandHandler : IRequestHandler<UpdateSubCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task Handle(UpdateSubCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.SubCategories.FindAsync(request.id);

        if (entity == null)
            throw new NotFoundException(nameof(SubCategory), request.id);

        entity.name = request.name;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
