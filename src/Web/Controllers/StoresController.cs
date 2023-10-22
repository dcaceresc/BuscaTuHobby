using Application.Maintainer.Stores.Commands.CreateStore;
using Application.Maintainer.Stores.Commands.ToggleStore;
using Application.Maintainer.Stores.Commands.UpdateStore;
using Application.Maintainer.Stores.Queries.GetStoreById;
using Application.Maintainer.Stores.Queries.GetStores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class StoresController : ApiController
{
    [HttpGet]
    public async Task<IList<StoreDto>> Get()
    {
        return await Mediator.Send(new GetStoresQuery());
    }
    [HttpGet("{id}")]
    public async Task<StoreVM> GetById(int id)
    {
        return await Mediator.Send(new GetStoreByIdQuery() { id = id });
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
    public async Task<ActionResult> Toggle(int id)
    {
        await Mediator.Send(new ToggleStoreCommand { id = id });

        return NoContent();
    }
}
