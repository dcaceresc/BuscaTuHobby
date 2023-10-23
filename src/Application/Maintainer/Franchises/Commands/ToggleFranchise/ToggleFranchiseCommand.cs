using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Franchises.Commands.ToggleFranchise;
public class ToggleFranchiseCommand : IRequest
{
    public int id { get; set; }
}

public class ToggleFranchiseCommandHandler : IRequestHandler<ToggleFranchiseCommand>
{
    private readonly IApplicationDbContext _context;

    public ToggleFranchiseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task Handle(ToggleFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Franchises.FindAsync(request.id);

        if (entity == null)
            throw new NotFoundException(nameof(Franchise), request.id);

        entity.active = !entity.active;

        await _context.SaveChangesAsync(cancellationToken);
    }
}