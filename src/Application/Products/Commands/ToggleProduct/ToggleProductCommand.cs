using Application.Common.Exceptions;
using Domain.Entities;

namespace Application.Products.Commands.ToggleProduct;

public class ToggleProductCommand : IRequest
{
    public int id { get; set; }

    public class ToggleProductCommandHandler : IRequestHandler<ToggleProductCommand>
    {
        private readonly IApplicationDbContext _context;

        public ToggleProductCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ToggleProductCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Products.FindAsync(request.id);

            if (entity == null)
                throw new NotFoundException(nameof(Product), request.id);

            entity.actve = !entity.actve;

            await _context.SaveChangesAsync(cancellationToken);

        }
    }
}
