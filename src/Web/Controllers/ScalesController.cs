using Application.Scales.Commands.CreateScale;
using Application.Scales.Commands.ToggleScale;
using Application.Scales.Commands.UpdateScale;
using Application.Scales.Queries.GetScaleById;
using Application.Scales.Queries.GetScales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ScalesController : ApiController
{
    [HttpGet]
    public async Task<IList<ScaleDto>> Get()
    {
        return await Mediator.Send(new GetScalesQuery());
    }

    [HttpGet("{id}")]
    public async Task<ScaleVM> GetById(int id)
    {
        return await Mediator.Send(new GetScaleByIdQuery() { id = id });
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateScaleCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateScaleCommand command)
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
        await Mediator.Send(new ToggleScaleCommand { id = id });

        return NoContent();
    }
}
