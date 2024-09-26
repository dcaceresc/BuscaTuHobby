using Application.Maintainer.Series.Commands.CreateSerie;
using Application.Maintainer.Series.Commands.ToggleSerie;
using Application.Maintainer.Series.Commands.UpdateSerie;
using Application.Maintainer.Series.Queries.GetSerieById;
using Application.Maintainer.Series.Queries.GetSeries;
using Application.Maintainer.Series.Queries.GetSeriesByFranchise;
using Domain.Common;

namespace WebAPI.Maintainer;

public class SeriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/series")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetSeries);
        group.MapGet("{id:guid}", GetSeriesById);
        group.MapGet("franchise/{id:guid}", GetSeriesByFranchise);
        group.MapPost("", CreateSeries);
        group.MapPut("{id:guid}", UpdateSeries);
        group.MapDelete("{id:guid}", ToggleSeries);

    }

    private static async Task<IResult> GetSeries(ISender sender) => Results.Ok(await sender.Send(new GetSeries()));

    private static async Task<IResult> GetSeriesById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetSerieById(id)));

    private static async Task<IResult> GetSeriesByFranchise(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetSeriesByFranchise(id)));

    private static async Task<IResult> CreateSeries(ISender sender, CreateSerie command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateSeries(ISender sender, Guid id, UpdateSerie command)
    {
        if (id != command.SerieId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la serie {command.SerieId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleSeries(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleSerie(id)));
}
