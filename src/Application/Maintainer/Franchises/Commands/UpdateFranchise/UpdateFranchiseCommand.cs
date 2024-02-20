namespace Application.Maintainer.Franchises.Commands.UpdateFranchise;
public record UpdateFranchiseCommand(Guid FranchiseId, string FranchiseName) : IRequest;

public class UpdateFranchiseCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateFranchiseCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Franchises.FindAsync([request.FranchiseId], cancellationToken);

        Guard.Against.NotFound(request.FranchiseId, entity);

        entity.Update(request.FranchiseName);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
