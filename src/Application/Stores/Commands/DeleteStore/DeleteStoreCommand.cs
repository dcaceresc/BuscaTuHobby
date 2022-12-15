using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Stores.Commands.DeleteStore;

public class DeleteStoreCommand : IRequest
{
    public int id { get; set; }

    public class DeleteStoreCommandHandler : IRequestHandler<DeleteStoreCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteStoreCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteStoreCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.Stores.FindAsync(request.id);

            if (entity == null)
            {
                throw new NotFoundException(nameof(Store), request.id);
            }

            _context.Stores.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}