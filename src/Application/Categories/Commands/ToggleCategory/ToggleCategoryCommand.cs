using Application.Common.Exceptions;
using Domain.Entities;


namespace Application.Categories.Commands.DeleteCategory;

public class ToggleCategoryCommand : IRequest
{
    public int id { get; set; }

    public class ToggleCategoryCommandHandler : IRequestHandler<ToggleCategoryCommand>
    {
        private readonly IApplicationDbContext _context;
        public ToggleCategoryCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleCategoryCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Categories.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Category), request.id);

            entity.active = !entity.active;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}