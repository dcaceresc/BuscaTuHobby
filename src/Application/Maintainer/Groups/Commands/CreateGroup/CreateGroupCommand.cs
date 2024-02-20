using Domain.Entities;

namespace Application.Maintainer.Groups.Commands.CreateGroup;
public record CreateGroupCommand(string GroupName) : IRequest<Guid>;

public class CreateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = Group.Create(request.GroupName);

        _context.Groups.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.GroupId;
    }
}
