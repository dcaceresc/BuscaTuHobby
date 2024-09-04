﻿using Application.Security.Account.Commands.AdminLogin;
using Application.Security.Account.Commands.CreateTokens;
using Application.Security.Account.Commands.UpdateRefreshToken;
using Application.Security.Account.Commands.UserLogin;

namespace WebAPI.Security;

public class AccountModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/security/account");

        group.MapPost("userLogin", UserLogin);
        group.MapPost("adminLogin", AdminLogin);
        group.MapPost("refreshToken", UpdateRefreshToken);
    }



    private static async Task<IResult> UserLogin(ISender sender, UserLogin command)
    {
        var result = await sender.Send(command);

        if (!result.Success)
            return Results.Ok(command);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }

    private static async Task<IResult> AdminLogin(ISender sender, AdminLogin command)
    {
        var result = await sender.Send(command);

        if (!result.Success)
            return Results.Ok(command);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateRefreshToken(ISender sender, UpdateRefreshToken command)
    {
        var result = await sender.Send(command);

        return Results.Ok(result);
    }
}
