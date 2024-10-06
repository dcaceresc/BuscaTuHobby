using Application.Maintainer.Products.Commands.CreateProduct;
using Application.Maintainer.Products.Commands.CreateProductImages;
using Application.Maintainer.Products.Commands.ToggleProduct;
using Application.Maintainer.Products.Commands.UpdateProduct;
using Application.Maintainer.Products.Queries.GetProductById;
using Application.Maintainer.Products.Queries.GetProducts;
using Domain.Common;

namespace WebAPI.Maintainer;

public class ProductsModule : CarterModule
{
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/maintainer/products")
            .RequireAuthorization(policy => policy.RequireRole("SuperAdmin", "Administrator"));

        group.MapGet("", GetProducts);
        group.MapGet("{id:guid}", GetProductById);
        group.MapPost("", CreateProduct);
        group.MapPost("{id:guid}/images", CreateProductImages);
        group.MapPut("{id:guid}", UpdateProduct);
        group.MapDelete("{id:guid}", ToggleProduct);

    }


    private static async Task<IResult> GetProducts(ISender sender) => Results.Ok(await sender.Send(new GetProducts()));

    private static async Task<IResult> GetProductById(ISender sender, Guid id) => Results.Ok(await sender.Send(new GetProductById(id)));

    private static async Task<IResult> CreateProduct(ISender sender, CreateProduct command) => Results.Ok(await sender.Send(command));

    private static async Task<IResult> CreateProductImages(ISender sender, Guid id, HttpRequest request)
    {
        if (request.Form.Files == null || request.Form.Files.Count == 0)
            return Results.BadRequest(new ApiResponse { Success = false, Message = "No se proporcionaron imágenes." });

        var images = new List<string>();

        foreach (var file in request.Form.Files)
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var base64String = Convert.ToBase64String(memoryStream.ToArray());
            images.Add(base64String);
        }

        return Results.Ok(await sender.Send(new CreateProductImages(id, images)));
    }

    private static async Task<IResult> UpdateProduct(ISender sender, Guid id, UpdateProduct command)
    {
        if (id != command.ProductId)
            return Results.Ok(new ApiResponse { Success = false, Message = $"La Id de la ruta {id} no coincide con la Id del producto {command.ProductId}" });

        return Results.Ok(await sender.Send(command));
    }

    private static async Task<IResult> ToggleProduct(ISender sender, Guid id) => Results.Ok(await sender.Send(new ToggleProduct(id)));
}

