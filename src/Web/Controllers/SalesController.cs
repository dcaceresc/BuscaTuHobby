﻿using Application.Sales.Commands.CreateSale;
using Application.Sales.Commands.ToggleSale;
using Application.Sales.Commands.UpdateSale;
using Application.Sales.Queries.GetSales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class SalesController : ApiController
{
    [HttpGet]
    public async Task<IList<SaleDto>> Get()
    {
        return await Mediator.Send(new GetSalesQuery());
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateSaleCommand command)
    {
        return await Mediator.Send(command);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, UpdateSaleCommand command)
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
        await Mediator.Send(new ToggleSaleCommand { id = id });

        return NoContent();
    }
}