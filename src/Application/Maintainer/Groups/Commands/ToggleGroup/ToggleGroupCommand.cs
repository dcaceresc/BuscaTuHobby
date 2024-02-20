namespace Application.Maintainer.Groups.Commands.ToggleGroup;

public record ToggleGroupCommand(Guid GroupId) : IRequest;

public class ToggleCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleGroupCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

        Guard.Against.NotFound(request.GroupId, entity);

        entity.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);
    }
}