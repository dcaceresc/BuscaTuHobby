using Application.Universes.Commands.CreateUniverse;
using Application.Universes.Commands.DeleteUniverse;
using Application.Universes.Commands.UpdateUniverse;
using Application.Universes.Queries.GetUniverses;
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
    public class UniversesController : ApiController
    {
        [HttpGet]
        public async Task<IList<UniverseVm>> Get()
        {
            return await Mediator.Send(new GetUniversesQuery());
        }

        [HttpPost(Name ="Create")]
        public async Task<ActionResult<int>> Create(CreateUniverseCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateUniverseCommand command)
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
            await Mediator.Send(new DeleteUniverseCommand { Id = id });

            return NoContent();
        }
    }
}
