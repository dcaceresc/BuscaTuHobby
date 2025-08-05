using Application.Security.Account.Commands.AdminLogin;
using Application.Security.Account.Commands.ConfirmEmail;
using Application.Security.Account.Commands.CreateTokens;
using Application.Security.Account.Commands.UpdateRefreshToken;
using Application.Security.Account.Commands.UserLogin;
using Application.Security.Account.Commands.UserRegister;

namespace WebAPI.Security;

public class AuthModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/security/auth");

        group.MapPost("userLogin", UserLogin);
        group.MapPost("adminLogin", AdminLogin);
        group.MapPost("userRegister", Register);
        group.MapPost("refreshToken", UpdateRefreshToken);
        group.MapPost("confirmEmail", ConfirmEmail);
    }

    private static async Task<IResult> UserLogin(IRequestDispatcher sender, UserLogin command)
    {
        var result = await sender.Send(command);

        if (!result.Success)
            return Results.Ok(result);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }

    private static async Task<IResult> AdminLogin(IRequestDispatcher sender, AdminLogin command)
    {
        var result = await sender.Send(command);

        if (!result.Success)
            return Results.Ok(result);

        var response = await sender.Send(new CreateTokens(command.Email));

        return Results.Ok(response);
    }

    private static async Task<IResult> Register(IRequestDispatcher sender, UserRegister command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateRefreshToken(IRequestDispatcher sender, UpdateRefreshToken command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> ConfirmEmail(IRequestDispatcher sender, ConfirmEmail command) => Results.Ok(await sender.Send(command));
}
