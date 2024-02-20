namespace Application.Maintainer.Scales.Commands.UpdateScale;

public record UpdateScaleCommand(Guid ScaleId, string ScaleName) : IRequest;

public class UpdateScaleCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateScaleCommand>
{
    private readonly IApplicationDbContext _context = context;

    public async Task Handle(UpdateScaleCommand request, CancellationToken cancellationToken)
    {
        var scale = await _context.Scales.FindAsync([request.ScaleId], cancellationToken);

        Guard.Against.NotFound(request.ScaleId, scale);

        scale.Update(request.ScaleName);

        await _context.SaveChangesAsync(cancellationToken);
    }
}