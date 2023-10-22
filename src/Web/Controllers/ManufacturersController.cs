using Application.Maintainer.Manufacturers.Commands.CreateManufacturer;
using Application.Maintainer.Manufacturers.Commands.ToggleManufacturer;
using Application.Maintainer.Manufacturers.Commands.UpdateManufacturer;
using Application.Maintainer.Manufacturers.Queries.GetManufacturerById;
using Application.Maintainer.Manufacturers.Queries.GetManufacturers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ManufacturersController : ApiController
{
    [HttpGet]
    public async Task<IList<ManufacturerDto>> Get()
    {
        return await Mediator.Send(new GetManufacturersQuery());
    }

    [HttpGet("{id}")]
    public async Task<ManufacturerVM> GetById(int id)
    {
        return await Mediator.Send(new GetManufacturerByIdQuery() { id = id });
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateManufacturerCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateManufacturerCommand command)
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
        await Mediator.Send(new ToggleManufacturerCommand { id = id });

        return NoContent();
    }
}

