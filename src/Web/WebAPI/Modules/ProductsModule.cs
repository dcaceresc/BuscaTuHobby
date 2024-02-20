using Application.Maintainer.Products.Commands.CreateProduct;
using Application.Maintainer.Products.Commands.ToggleProduct;
using Application.Maintainer.Products.Commands.UpdateProduct;
using Application.Maintainer.Products.Queries.GetProductById;
using Application.Maintainer.Products.Queries.GetProducts;

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


    private static async Task<IResult> GetProducts(ISender sender) => Results.Ok(await sender.Send(new GetProductsQuery()));

    private static async Task<IResult> GetProductById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetProductByIdQuery(id)));

    private static async Task<IResult> CreateProduct(ISender sender, CreateProductCommand command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> UpdateProduct(ISender sender, Guid id, UpdateProductCommand command)
    {
        if (id != command.ProductId)
            return Results.BadRequest();

        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ToggleProduct(ISender sender, Guid id)
    {
        await sender.Send(new ToggleProductCommand(id));

        return Results.NoContent();
    }
}
