using Application.Stores.Commands.CreateStore;
using Application.Stores.Commands.DeleteStore;
using Application.Stores.Commands.UpdateStore;
using Application.Stores.Queries.GetStores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Authorize]
public class StoresController : ApiController
{
    [HttpGet]
    public async Task<IList<StoreVm>> Get()
    {
        return await Mediator.Send(new GetStoresQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateStoreCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateStoreCommand command)
    {
        if (id != command.id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteStoreCommand { id = id });

        return NoContent();
    }
}
