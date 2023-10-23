using Application.Maintainer.Series.Commands.CreateSerie;
using Application.Maintainer.Series.Commands.ToggleSerie;
using Application.Maintainer.Series.Commands.UpdateSerie;
using Application.Maintainer.Series.Queries.GetSerieById;
using Application.Maintainer.Series.Queries.GetSeries;
using Application.Maintainer.Series.Queries.GetSeriesByFranchise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SeriesController : ApiController
{
    [HttpGet]
    public async Task<IList<SerieDto>> Get()
    {
        return await Mediator.Send(new GetSeriesQuery());
    }

    [HttpGet("{id}")]
    public async Task<SerieVM> GetById(int id)
    {
        return await Mediator.Send(new GetSerieByIdQuery() { id = id });
    }

    [HttpGet("ByFranchise/{franchiseId}")]
    public async Task<IList<SerieByFranchiseDto>> GetByFranchise(int franchiseId)
    {
        return await Mediator.Send(new GetSeriesByFranchiseQuery() { franchiseId = franchiseId });
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateSerieCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateSerieCommand command)
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
        await Mediator.Send(new ToggleSerieCommand { id = id });

        return NoContent();
    }
}
