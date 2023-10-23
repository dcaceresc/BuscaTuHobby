using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Maintainer.Franchises.Commands.UpdateFranchise;
public class UpdateFranchiseCommand : IRequest
{
    public int id { get; set; }
    public string name { get; set; } = default!;
}

public class UpdateFranchiseCommandHandler : IRequestHandler<UpdateFranchiseCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateFranchiseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task Handle(UpdateFranchiseCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Franchises.FindAsync(request.id);

        if (entity == null)
            throw new NotFoundException(nameof(Franchise), request.id);

        entity.name = request.name;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
