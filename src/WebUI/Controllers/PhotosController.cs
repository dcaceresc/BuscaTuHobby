using Application.Photos.Commands.CreatePhoto;
using Application.Photos.Commands.DeletePhoto;
using Application.Photos.Commands.UpdatePhoto;
using Application.Photos.Queries.GetPhotos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ApiController
    {
        [HttpGet]
        public async Task<IList<PhotoVm>> Get()
        {
            return await Mediator.Send(new GetPhotosQuery());
        }

        // POST api/<PhotosController>
        [HttpPost]
        public async Task<ActionResult<int>> Create(CreatePhotoCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdatePhotoCommand command)
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
            await Mediator.Send(new DeletePhotoCommand { Id = id });

            return NoContent();
        }
    }
}
