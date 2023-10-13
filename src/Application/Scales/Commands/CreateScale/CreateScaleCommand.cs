using Domain.Entities;

namespace Application.Scales.Commands.CreateScale;

public class CreateScaleCommand : IRequest<int>
{
    public string name { get; set; } = default!;
    public string acronym { get; set; } = default!;

    public class CreateScaleCommandHandler : IRequestHandler<CreateScaleCommand, int>
    {
        private readonly IApplicationDbContext _context;

        public CreateScaleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<int> Handle(CreateScaleCommand request, CancellationToken cancellationToken)
        {
            var entity = new Scale
            {
                name = request.name,
                acronym = request.acronym
            };

            _context.Scales.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity.id;
        }
    }
}