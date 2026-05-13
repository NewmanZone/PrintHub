using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueueController : ControllerBase
{
    private static readonly Action<ILogger, string?, Exception?> LogGettingQueueStatus =
        LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(1, nameof(LogGettingQueueStatus)),
            "Getting queue status for user {UserId}");

    private static readonly Action<ILogger, int, string?, Exception?> LogAddingItems =
        LoggerMessage.Define<int, string?>(
            LogLevel.Information,
            new EventId(2, nameof(LogAddingItems)),
            "Adding {Count} items to queue for user {UserId}");

    private readonly ILogger<QueueController> _logger;

    public QueueController(ILogger<QueueController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get current queue status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(QueueStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetQueueStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        LogGettingQueueStatus(_logger, userId, null);

        return Ok(new QueueStatusResponse
        {
            TotalJobs = 0,
            PendingJobs = 0,
            ActiveJobs = 0,
            CompletedJobs = 0,
            LastUpdated = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Add items to the print queue
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(QueueAddResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult AddToQueue([FromBody] AddToQueueRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { error = "Queue items are required" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        LogAddingItems(_logger, request.Items.Count, userId, null);

        return Created("/api/queue/status", new QueueAddResponse
        {
            JobId = "job-" + Guid.NewGuid().ToString("N"),
            ItemsAdded = request.Items.Count,
            EstimatedTimeMinutes = request.Items.Count * 30,
            CreatedAt = DateTime.UtcNow
        });
    }
}

public class QueueStatusResponse
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int ActiveJobs { get; set; }
    public int CompletedJobs { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class AddToQueueRequest
{
    public List<QueueItemRequest> Items { get; set; } = new();
}

public class QueueItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string? Notes { get; set; }
}

public class QueueAddResponse
{
    public string JobId { get; set; } = string.Empty;
    public int ItemsAdded { get; set; }
    public int EstimatedTimeMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
}
