using Microsoft.AspNetCore.Mvc;

namespace Cuppie.Api.Controllers;

public class PageDto
{
    public string Content { get; set; } = string.Empty;
}

[ApiController]
[Route("pages")]
public class PageController: ControllerBase
{
    [HttpPost("save")]
    public IActionResult SavePage([FromBody] PageDto dto)
    { 
        return Ok(dto.Content);
    }
}