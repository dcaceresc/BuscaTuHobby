using Domain.Entities;

namespace Application.Series.Commands.CreateSerie;

public class CreateSerieCommand : IRequest<int>
{
    public string name { get; set; } = default!;

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
                active = true
            };

            _context.Series.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }
}

