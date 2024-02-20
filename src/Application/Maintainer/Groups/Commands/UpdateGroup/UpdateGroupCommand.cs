namespace Application.Maintainer.Groups.Commands.UpdateGroup;

public record UpdateGroupCommand(Guid GroupId, string GroupName) : IRequest;

public class UpdateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

        Guard.Against.NotFound(request.GroupId, entity);

        entity.Update(request.GroupName);

        await _context.SaveChangesAsync(cancellationToken);
    }
}



