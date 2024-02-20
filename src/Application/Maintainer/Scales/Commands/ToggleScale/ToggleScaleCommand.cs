namespace Application.Maintainer.Scales.Commands.ToggleScale;

public record ToggleScaleCommand(Guid ScaleId) : IRequest;

public class ToggleScaleCommandHandler(IApplicationDbContext context) : IRequestHandler<ToggleScaleCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(ToggleScaleCommand request, CancellationToken cancellationToken)
    {
        var scale = await _context.Scales.FindAsync([request.ScaleId], cancellationToken);

        Guard.Against.NotFound(request.ScaleId, scale);

        scale.ToggleActive();

        await _context.SaveChangesAsync(cancellationToken);

    }
}