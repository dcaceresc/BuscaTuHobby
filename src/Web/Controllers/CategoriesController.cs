using Application.Categories.Commands.CreateCategory;
using Application.Categories.Commands.CreateSubCategory;
using Application.Categories.Commands.DeleteCategory;
using Application.Categories.Commands.ToggleSubCategory;
using Application.Categories.Commands.UpdateCategory;
using Application.Categories.Commands.UpdateSubCategory;
using Application.Categories.Queries.GetCategories;
using Application.Categories.Queries.GetCategoryById;
using Application.Categories.Queries.GetSubCategoriesByCategory;
using Application.Categories.Queries.GetSubCategoryById;
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
    public async Task<CategoryVM> GetById(int id)
    {
        return await Mediator.Send(new GetCategoryByIdQuery() { id = id});
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateCategoryCommand command)
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
    public async Task<ActionResult> Toggle(int id)
    {
        await Mediator.Send(new ToggleCategoryCommand { id = id });

        return NoContent();
    }


    #region SubCategories


    [HttpGet("{categoryId}/SubCategories")]
    public async Task<IList<SubCategoryDto>> GetSubCategories(int categoryId)
    {
        return await Mediator.Send(new GetSubCategoriesByCategoryQuery(){ categoryId = categoryId});
    }

    [HttpGet("{categoryId}/SubCategories/{id}")]
    public async Task<SubCategoryVM> GetSubCategoriesById(int id)
    {
        return await Mediator.Send(new GetSubCategoryByIdQuery() { id = id});
    }

    [HttpPost("{categoryId}/SubCategories")]
    public async Task<int> CreateSubCategory(CreateSubCategoryCommand command)
    {
        return await Mediator.Send(command);
    }


    [HttpPut("{categoryId}/SubCategories/{id}")]
    public async Task<ActionResult> Update(int id, UpdateSubCategoryCommand command)
    {
        if (id != command.id)
        {
            return BadRequest();
        }

        await Mediator.Send(command);

        return NoContent();
    }

    [HttpDelete("{categoryId}/SubCategories/{id}")]
    public async Task<ActionResult> ToggleSubCategory(int id)
    {
        await Mediator.Send(new ToggleSubCategoryCommand { id = id });

        return NoContent();
    }


    #endregion


}