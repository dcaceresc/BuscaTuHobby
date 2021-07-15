using Application.Grades.Queries.GetGrades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Grades.Commands.CreateGrade;
using Application.Grades.Commands.UpdateGrade;
using Application.Grades.Commands.DeleteGrade;

namespace WebUI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ApiController
    {
        [HttpGet]
        public async Task<IList<GradeVm>> Get()
        {
            return await Mediator.Send(new GetGradesQuery());
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateGradeCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateGradeCommand command)
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
            await Mediator.Send(new DeleteGradeCommand { Id = id });

            return NoContent();
        }



    }
}
