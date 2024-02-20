using Domain.Entities;

namespace Application.Maintainer.Scales.Commands.CreateScale;

public record CreateScaleCommand(string ScaleName) : IRequest<Guid>;

public class CreateScaleCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateScaleCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;

    public async Task<Guid> Handle(CreateScaleCommand request, CancellationToken cancellationToken)
    {
        var scale = Scale.Create(request.ScaleName);

        _context.Scales.Add(scale);

        await _context.SaveChangesAsync(cancellationToken);

        return scale.ScaleId;
    }
}