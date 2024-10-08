﻿using Application.Security.Users.Commands.CreateUser;
using Application.Security.Users.Commands.ToggleUser;
using Application.Security.Users.Commands.UpdateUser;
using Application.Security.Users.Queries.GetUserById;
using Application.Security.Users.Queries.GetUsers;

namespace WebAPI.Security;

public class UserModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/security/users")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetUsers);
        group.MapGet("{id:guid}", GetUserById);
        group.MapPost("", CreateUser);
        group.MapPut("{id:guid}", UpdateUser);
        group.MapDelete("{id:guid}", ToggleUser);
    }

    private static async Task<IResult> GetUsers(ISender sender) => Results.Ok(await sender.Send(new GetUsers()));

    private static async Task<IResult> GetUserById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetUserById(id)));

    private static async Task<IResult> CreateUser(ISender sender, CreateUser command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateUser(ISender sender, Guid id, UpdateUser command)
    {
        var result = await sender.Send(command);

        return Results.Ok(result);
    }

    private static async Task<IResult> ToggleUser(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleUser(id)));
}
