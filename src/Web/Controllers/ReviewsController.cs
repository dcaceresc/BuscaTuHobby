﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ApiController
{
    [HttpGet]
    public IActionResult Index()
    {
        return NoContent();
    }
}