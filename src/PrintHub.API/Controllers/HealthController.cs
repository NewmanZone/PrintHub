using Microsoft.AspNetCore.Mvc;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "healthy",
        service = "PrintHub.API",
        timestamp = DateTime.UtcNow
    });
}
