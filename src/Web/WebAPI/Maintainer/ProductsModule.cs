using Application.Maintainer.Products.Commands.CreateProduct;
using Application.Maintainer.Products.Commands.ToggleProduct;
using Application.Maintainer.Products.Commands.UpdateProduct;
using Application.Maintainer.Products.Queries.GetProductById;
using Application.Maintainer.Products.Queries.GetProducts;
using Domain.Common;

namespace WebAPI.Modules;

public class ProductsModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/products").RequireAuthorization();

        group.MapGet("", GetProducts);
        group.MapGet("{id:guid}", GetProductById);
        group.MapPost("", CreateProduct);
        group.MapPut("{id:guid}", UpdateProduct);
        group.MapDelete("{id:guid}", ToggleProduct);

    }


    private static async Task<IResult> GetProducts(ISender sender) => Results.Ok(await sender.Send(new GetProducts()));

    private static async Task<IResult> GetProductById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetProductById(id)));

    private static async Task<IResult> CreateProduct(ISender sender, CreateProduct command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateProduct(ISender sender, Guid id, UpdateProduct command)
    {
        if (id != command.ProductId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id del producto {command.ProductId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleProduct(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleProduct(id)));
}
