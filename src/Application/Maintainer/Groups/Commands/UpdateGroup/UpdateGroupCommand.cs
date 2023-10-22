using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public class UpdateGroupCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateGroupCommand>
{
    private readonly IApplicationDbContext _context;
    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Groups.FindAsync(request.id);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Group), request.id);
        }

        entity.name = request.name;

        await _context.SaveChangesAsync(cancellationToken);
    }
}



