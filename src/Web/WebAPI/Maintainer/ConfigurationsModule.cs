using Application.Maintainer.Configurations.Commands.CreateConfiguration;
using Application.Maintainer.Configurations.Commands.ToggleConfiguration;
using Application.Maintainer.Configurations.Commands.UpdateConfiguration;
using Application.Maintainer.Configurations.Queries.GetConfigurationById;
using Application.Maintainer.Configurations.Queries.GetConfigurations;
using Domain.Common;

namespace WebAPI.Maintainer;

public class ConfigurationsModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/configurations")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetConfigurations);
        group.MapGet("{id:guid}", GetConfigurationById);
        group.MapPost("", CreateConfiguration);
        group.MapPut("{id:guid}", UpdateConfiguration);
        group.MapDelete("{id:guid}", ToggleConfiguration);

    }

    private static async Task<IResult> GetConfigurations(ISender sender) => Results.Ok(await sender.Send(new GetConfigurations()));

    private static async Task<IResult> GetConfigurationById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetConfigurationById(id)));

    private static async Task<IResult> CreateConfiguration(ISender sender, CreateConfiguration command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateConfiguration(ISender sender, Guid id, UpdateConfiguration command)
    {
        if (id != command.ConfigurationId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id de la configuración {command.ConfigurationId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleConfiguration(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleConfiguration(id)));

}
