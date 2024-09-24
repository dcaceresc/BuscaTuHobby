using Application.Maintainer.Cities.Commands.CreateCity;
using Application.Maintainer.Cities.Commands.ToggleCity;
using Application.Maintainer.Cities.Commands.UpdateCity;
using Application.Maintainer.Cities.Queries.GetCities;
using Application.Maintainer.Cities.Queries.GetCityById;
using Domain.Common;

namespace WebAPI.Maintainer;

public class CitiesModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/cities").RequireAuthorization();

        group.MapGet("", GetCities);
        group.MapGet("{id:guid}", GetCityById);
        group.MapPost("", CreateCity);
        group.MapPut("{id:guid}", UpdateCity);
        group.MapDelete("{id:guid}", ToggleCity);
    }

    private static async Task<IResult> GetCities(ISender sender) => Results.Ok(await sender.Send(new GetCities()));

    private static async Task<IResult> GetCityById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetCityById(id)));

    private static async Task<IResult> CreateCity(ISender sender, CreateCity command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateCity(ISender sender, Guid id, UpdateCity command)
    {
        if (id != command.CityId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La id de la ruta {id} no coincide con la de la ciudad {command.CityId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleCity(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleCity(id)));


}
