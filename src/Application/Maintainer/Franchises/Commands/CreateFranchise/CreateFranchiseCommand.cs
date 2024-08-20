namespace Application.Maintainer.Franchises.Commands.CreateFranchise;
public record CreateFranchiseCommand(string FranchiseName) : IRequest<Guid>;

public class CreateFranchiseCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateFranchiseCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = Franchise.Create(request.FranchiseName);

        _context.Franchises.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.FranchiseId;
    }
}
