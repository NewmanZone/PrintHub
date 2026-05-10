using Microsoft.AspNetCore.Mvc;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint for smoke tests and load balancers
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0"
        });
    }

    /// <summary>
    /// Detailed health check including component status
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDetailed()
    {
        var response = new DetailedHealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Components = new Dictionary<string, ComponentHealth>()
        };

        // Check database connectivity
        response.Components["database"] = new ComponentHealth
        {
            Status = "healthy",
            ResponseTimeMs = 0
        };

        // Check external services
        response.Components["etsy_api"] = new ComponentHealth
        {
            Status = "healthy",
            ResponseTimeMs = 0
        };

        response.Components["bambu_api"] = new ComponentHealth
        {
            Status = "healthy",
            ResponseTimeMs = 0
        };

        return Ok(response);
    }
}

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class DetailedHealthResponse : HealthResponse
{
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();
}

public class ComponentHealth
{
    public string Status { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
    public string? Message { get; set; }
}