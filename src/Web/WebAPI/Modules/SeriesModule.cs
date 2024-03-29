﻿using Application.Maintainer.Series.Commands.CreateSerie;
using Application.Maintainer.Series.Commands.ToggleSerie;
using Application.Maintainer.Series.Commands.UpdateSerie;
using Application.Maintainer.Series.Queries.GetSerieById;
using Application.Maintainer.Series.Queries.GetSeries;
using Application.Maintainer.Series.Queries.GetSeriesByFranchise;

namespace WebAPI.Modules;

public class SeriesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/series").RequireAuthorization();

        group.MapGet("", GetSeries);
        group.MapGet("{id:guid}", GetSeriesById);
        group.MapGet("franchise/{id:guid}", GetSeriesByFranchise);
        group.MapPost("", CreateSeries);
        group.MapPut("{id:guid}", UpdateSeries);
        group.MapDelete("{id:guid}", ToggleSeries);

    }

    private static async Task<IResult> GetSeries(ISender sender) => Results.Ok(await sender.Send(new GetSeriesQuery()));

    private static async Task<IResult> GetSeriesById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetSerieByIdQuery(id)));

    private static async Task<IResult> GetSeriesByFranchise(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetSeriesByFranchiseQuery(id)));

    private static async Task<IResult> CreateSeries(ISender sender, CreateSerieCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateSeries(ISender sender, Guid id, UpdateSerieCommand command)
    {
        if (id != command.SerieId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleSeries(ISender sender, Guid id)
    {
        await sender.Send(new ToggleSerieCommand(id));

        return Results.NoContent();
    }
}
