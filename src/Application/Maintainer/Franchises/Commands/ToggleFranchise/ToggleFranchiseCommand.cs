namespace Application.Maintainer.Franchises.Commands.ToggleFranchise;
public record ToggleFranchiseCommand(Guid FranchiseId) : IRequest;

public class ToggleFranchiseCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleFranchiseCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Franchises.FindAsync([request.FranchiseId], cancellationToken);

        Guard.Against.NotFound(request.FranchiseId, entity);

        entity.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);
    }
}