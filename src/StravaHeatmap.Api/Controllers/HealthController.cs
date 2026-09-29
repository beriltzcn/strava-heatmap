using Microsoft.AspNetCore.Mvc;

namespace StravaHeatmap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new 
            {
            status = "ok",
            time = DateTimeOffset.UtcNow
        });
    }
}
