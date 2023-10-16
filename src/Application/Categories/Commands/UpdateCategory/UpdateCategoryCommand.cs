using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Categories.FindAsync(request.id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Category), request.id);
        }

        entity.name = request.name;

        await _context.SaveChangesAsync(cancellationToken);
    }
}



