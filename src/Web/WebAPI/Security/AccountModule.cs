using Application.Security.Account.Commands.AdminLogin;
using Application.Security.Account.Commands.CreateTokens;
using Application.Security.Account.Commands.UserLogin;

namespace WebAPI.Security;

public class AccountModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/account");

        group.MapPost("userlogin", UserLogin);
        group.MapPost("adminlogin", AdminLogin);
    }



    private static async Task<IResult> UserLogin(ISender sender, UserLogin command)
    {
        var result = await sender.Send(command);

        if (result is null)
            return Results.Ok(command);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }

    private static async Task<IResult> AdminLogin(ISender sender, AdminLogin command)
    {
        var result = await sender.Send(command);

        if (result is null)
            return Results.Ok(command);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }
}
