﻿using Application.Gunplas.Commands.CreateGunpla;
using Application.Gunplas.Commands.ToggleGunpla;
using Application.Gunplas.Commands.UpdateGunpla;
using Application.Gunplas.Queries.GetGunplas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GunplasController : ApiController
{
    [HttpGet]
    public async Task<IList<GunplaDto>> Get()
    {
        return await Mediator.Send(new GetGunplasQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateGunplaCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateGunplaCommand command)
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
        await Mediator.Send(new ToggleGunplaCommand { id = id });

        return NoContent();
    }


}