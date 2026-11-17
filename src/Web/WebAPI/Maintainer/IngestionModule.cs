using Application.Maintainer.Ingestion.Commands.StartIngestionRun;
using Application.Maintainer.Ingestion.Queries.GetIngestionStatus;

namespace WebAPI.Maintainer;

// Rutas nuevas y aditivas del backoffice: el disparo manual de una ingesta y su estado. La protección es la misma que
// la del resto de módulos del mantenedor, así que la ingesta no amplía quién puede tocar el catálogo.
public class IngestionModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/ingestion")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapPost("runs", StartIngestionRun);
        group.MapGet("status", GetIngestionStatus);
    }

    private static async Task<IResult> StartIngestionRun(IRequestDispatcher sender, StartIngestionRun command,
        CancellationToken cancellationToken) =>
        Results.Ok(await sender.Send(command, cancellationToken));

    private static async Task<IResult> GetIngestionStatus(IRequestDispatcher sender, CancellationToken cancellationToken) =>
        Results.Ok(await sender.Send(new GetIngestionStatus(), cancellationToken));
}
