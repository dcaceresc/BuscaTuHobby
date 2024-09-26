using Application.Maintainer.Manufacturers.Commands.CreateManufacturer;
using Application.Maintainer.Manufacturers.Commands.ToggleManufacturer;
using Application.Maintainer.Manufacturers.Commands.UpdateManufacturer;
using Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
using Application.Maintainer.Manufacturers.Queries.GetManufacturers;
using Domain.Common;

namespace WebAPI.Maintainer;

public class ManufacturersModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("api/manufacturers")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        groups.MapGet("", GetManufacturers);
        groups.MapGet("{id:guid}", GetManufacturerById);
        groups.MapPost("", CreateManufacturer);
        groups.MapPut("{id:guid}", UpdateManufacturer);
        groups.MapDelete("{id:guid}", ToggleManufacturer);

    }

    private static async Task<IResult> GetManufacturers(ISender sender) => Results.Ok(await sender.Send(new GetManufacturers()));

    private static async Task<IResult> GetManufacturerById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetManufacturerById(id)));
    private static async Task<IResult> CreateManufacturer(ISender sender, CreateManufacturer command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateManufacturer(ISender sender, Guid id, UpdateManufacturer command)
    {
        if (id != command.ManufacturerId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la del fabricante {command.ManufacturerId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleManufacturer(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleManufacturer(id)));
}
