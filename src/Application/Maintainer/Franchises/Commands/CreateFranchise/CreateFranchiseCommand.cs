using Domain.Entities;

namespace Application.Maintainer.Franchises.Commands.CreateFranchise;
public class CreateFranchiseCommand : IRequest<int>
{
    public string name { get; set; } = default!;
}

public class CreateFranchiseCommandHandler : IRequestHandler<CreateFranchiseCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateFranchiseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<int> Handle(CreateFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = new Franchise
        {
            name = request.name,
            active = true
        };

        _context.Franchises.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.id;
    }
}
