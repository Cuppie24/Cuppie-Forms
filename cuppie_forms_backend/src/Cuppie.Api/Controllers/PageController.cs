using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cuppie.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pages")]
public class PageController: ControllerBase
{
    private const string Message = "Пароль от wifi на даче: qwert123";

    [HttpGet("home/get")]
    public IActionResult GetHomePage()
    {
        Response.ContentType = "application/json";
        return Ok(new {message = Message});
    }
}