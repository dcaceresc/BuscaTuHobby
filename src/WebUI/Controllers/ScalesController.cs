using Application.Scales.Commands.CreateScale;
using Application.Scales.Commands.DeleteScale;
using Application.Scales.Commands.UpdateScale;
using Application.Scales.Queries.GetScales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Controllers;

[Authorize]
public class ScalesController : ApiController
{
    [HttpGet]
    public async Task<IList<ScaleVm>> Get()
    {
        return await Mediator.Send(new GetScalesQuery());
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
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteScaleCommand { id = id });

        return NoContent();
    }
}

