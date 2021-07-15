using Application.Series.Commands.CreateSerie;
using Application.Series.Commands.DeleteSerie;
using Application.Series.Commands.UpdateSerie;
using Application.Series.Queries.GetSeries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesController : ApiController
    {
        [HttpGet]
        public async Task<IList<SerieVm>> Get()
        {
            return await Mediator.Send(new GetSeriesQuery());
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateSerieCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateSerieCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await Mediator.Send(command);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await Mediator.Send(new DeleteSerieCommand { Id = id });

            return NoContent();
        }
    }
}
