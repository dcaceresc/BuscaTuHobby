using Application.Photos.Commands.CreatePhoto;
using Application.Photos.Commands.DeletePhoto;
using Application.Photos.Commands.UpdatePhoto;
using Application.Photos.Queries.GetPhotos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PhotosController : ApiController
{
    [HttpGet]
    public async Task<IList<PhotoVm>> Get()
    {
        return await Mediator.Send(new GetPhotosQuery());
    }



    [HttpPost]
    public async Task<ActionResult<int>> Create(CreatePhotoCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdatePhotoCommand command)
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
        await Mediator.Send(new DeletePhotoCommand { id = id });

        return NoContent();
    }
}