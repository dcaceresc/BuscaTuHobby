using Application.Manufacturers.Commands.CreateManufacturer;
using Application.Manufacturers.Commands.DeleteManufacturer;
using Application.Manufacturers.Commands.UpdateManufacturer;
using Application.Manufacturers.Queries.GetManufacturers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Authorize]
public class ManufacturersController : ApiController
{
    [HttpGet]
    public async Task<IList<ManufacturerVm>> Get()
    {
        return await Mediator.Send(new GetManufacturersQuery());
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
    public async Task<ActionResult> Delete(int id)
    {
        await Mediator.Send(new DeleteManufacturerCommand { id = id });

        return NoContent();
    }
}

