using Application.Common.Exceptions;
using Domain.Entities;


namespace Application.Maintainer.Groups.Commands.ToggleGroup;

public class ToggleGroupCommand : IRequest
{
    public int id { get; set; }

    public class ToggleCategoryCommandHandler : IRequestHandler<ToggleGroupCommand>
    {
        private readonly IApplicationDbContext _context;
        public ToggleCategoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleGroupCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Groups.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Group), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}