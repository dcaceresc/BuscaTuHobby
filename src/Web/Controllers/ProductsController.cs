using Application.Maintainer.Products.Commands.CreateProduct;
using Application.Maintainer.Products.Commands.ToggleProduct;
using Application.Maintainer.Products.Commands.UpdateProduct;
using Application.Maintainer.Products.Queries.GetProductById;
using Application.Maintainer.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ProductsController : ApiController
{
    [HttpGet]
    public async Task<IList<ProductDto>> Get()
    {
        return await Mediator.Send(new GetProductsQuery());
    }

    [HttpGet("{id}")]
    public async Task<ProductVM> GetById(int id)
    {
        return await Mediator.Send(new GetProductByIdQuery() { id = id});
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateProductCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateProductCommand command)
    {
        if (id != command.id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Toggle(int id)
    {
        await Mediator.Send(new ToggleProductCommand { id = id });

        return NoContent();
    }


}