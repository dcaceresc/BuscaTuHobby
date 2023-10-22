using Application.Maintainer.Categories.Commands.CreateCategory;
using Application.Maintainer.Categories.Commands.ToggleCategory;
using Application.Maintainer.Categories.Commands.UpdateCategory;
using Application.Maintainer.Categories.Queries.GetCategories;
using Application.Maintainer.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ApiController
{


    [HttpGet]
    public async Task<IList<CategoryDto>> Get()
    {
        return await Mediator.Send(new GetCategoriesQuery());
    }

    [HttpGet("{id}")]
    public async Task<CategoryVM> GetSubCategoriesById(int id)
    {
        return await Mediator.Send(new GetCategoryByIdQuery() { id = id });
    }

    [HttpPost]
    public async Task<int> CreateSubCategory(CreateCategoryCommand command)
    {
        return await Mediator.Send(command);
    }


    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateCategoryCommand command)
    {
        if (id != command.id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> ToggleSubCategory(int id)
    {
        await Mediator.Send(new ToggleCategoryCommand { id = id });

        return NoContent();
    }


}