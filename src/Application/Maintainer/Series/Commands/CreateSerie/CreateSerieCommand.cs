using Domain.Entities;

namespace Application.Maintainer.Series.Commands.CreateSerie;

public class CreateSerieCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public int franchiseId { get; set; }
    public class CreateSerieCommandHandler : IRequestHandler<CreateSerieCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateSerieCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> Handle(CreateSerieCommand request, CancellationToken cancellationToken)
        {
            var entity = new Serie
            {
                name = request.name,
                franchiseId = request.franchiseId,
                active = true
            };

            _context.Series.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }
}

