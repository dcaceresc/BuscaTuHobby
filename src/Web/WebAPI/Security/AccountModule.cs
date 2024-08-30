
namespace WebAPI.Security;

public class AccountModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/security/account");
    }



    private static async Task<IResult> Login(ISender sender, Login command)
    {
        var result = await sender.Send(command);

        if (result is null)
            return Results.Unauthorized(new ApiResponse { Success = false, Message = "Usuario o contraseña incorrectos" });

        return Results.Ok(result);
    }
}
