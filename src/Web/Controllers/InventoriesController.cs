using Application.Inventories.Commands.CreateInventory;
using Application.Inventories.Commands.ToggleInventory;
using Application.Inventories.Commands.UpdateInventory;
using Application.Inventories.Queries.GetInventories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class InventoriesController : ApiController
{
    [HttpGet]
    public async Task<IList<InventoryDto>> Get()
    {
        return await Mediator.Send(new GetInventoriesQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateInventoryCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateInventoryCommand command)
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
        await Mediator.Send(new ToggleInventoryCommand { id = id });

        return NoContent();
    }
}
