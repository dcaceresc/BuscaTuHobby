using Application.Maintainer.Franchises.Commands.CreateFranchise;
using Application.Maintainer.Franchises.Commands.ToggleFranchise;
using Application.Maintainer.Franchises.Commands.UpdateFranchise;
using Application.Maintainer.Franchises.Queries.GetFranchiseById;
using Application.Maintainer.Franchises.Queries.GetFranchises;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;
[Route("api/[controller]")]
[ApiController]
public class FranchisesController : ApiController
{
    [HttpGet]
    public async Task<IList<FranchiseDto>> Get()
    {
        return await Mediator.Send(new GetFranchisesQuery());
    }

    [HttpGet("{id}")]
    public async Task<FranchiseVM> GetById(int id)
    {
        return await Mediator.Send(new GetFranchiseByIdQuery() { id = id });
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateFranchiseCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateFranchiseCommand command)
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
        await Mediator.Send(new ToggleFranchiseCommand { id = id });

        return NoContent();
    }
}
