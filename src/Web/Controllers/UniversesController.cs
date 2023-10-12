using Application.Universes.Commands.CreateUniverse;
using Application.Universes.Commands.ToggleUniverse;
using Application.Universes.Commands.UpdateUniverse;
using Application.Universes.Queries.GetUniverseById;
using Application.Universes.Queries.GetUniverses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UniversesController : ApiController
{
    [HttpGet]
    public async Task<IList<UniverseDto>> Get()
    {
        return await Mediator.Send(new GetUniversesQuery());
    }

    [HttpGet("{id}")]
    public async Task<UniverseVM> GetById(int id)
    {
        return await Mediator.Send(new GetUniverseByIdQuery() { id = id});
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateUniverseCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateUniverseCommand command)
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
        await Mediator.Send(new ToggleUniverseCommand { id = id });

        return NoContent();
    }
}

