using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services.Etsy;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;
    private readonly ILogger<ShopsController> _logger;

    public ShopsController(IShopService shopService, ILogger<ShopsController> logger)
    {
        _shopService = shopService;
        _logger = logger;
    }

    /// <summary>
    /// List all connected shops for the authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ShopsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetShops()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        _logger.LogInformation("Getting shops for user {UserId}", userId);
        
        var shops = await _shopService.GetShopsAsync(userId.Value);
        
        return Ok(new ShopsResponse
        {
            Shops = shops.Select(s => new ShopDto
            {
                Id = s.Id,
                Provider = s.Provider,
                ExternalId = s.ExternalId,
                ShopName = s.ShopName,
                IsActive = s.IsActive,
                LastSyncAt = s.LastSyncAt
            }).ToList()
        });
    }

    /// <summary>
    /// Initiate Etsy OAuth flow.
    /// </summary>
    [HttpPost("connect/etsy")]
    [ProducesResponseType(typeof(ConnectResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConnectEtsy([FromBody] ConnectEtsyRequest? request = null)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        _logger.LogInformation("Initiating Etsy connect for user {UserId}", userId);
        
        var result = await _shopService.InitiateEtsyConnectAsync(userId.Value, request?.ReturnUrl);
        
        return Ok(new ConnectResponseDto { AuthUrl = result.AuthUrl });
    }

    /// <summary>
    /// Etsy OAuth callback (handled by frontend redirect).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("etsy/callback")]
    [ProducesResponseType(typeof(CallbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EtsyCallback([FromBody] EtsyCallbackRequest request)
    {
        if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State))
        {
            return BadRequest(new { error = "Code and state are required" });
        }
        
        _logger.LogInformation("Handling Etsy callback with state {State}", request.State);
        
        try
        {
            var result = await _shopService.HandleEtsyCallbackAsync(request.Code, request.State);
            
            return Ok(new CallbackResponseDto
            {
                ShopId = result.ShopId,
                ShopName = result.ShopName,
                Connected = result.Connected
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("OAuth state"))
        {
            _logger.LogWarning(ex, "Invalid OAuth state received");
            return BadRequest(new { error = "Invalid or expired OAuth state" });
        }
    }

    /// <summary>
    /// Disconnect a shop.
    /// </summary>
    [HttpDelete("{shopId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShop(Guid shopId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        _logger.LogInformation("Deleting shop {ShopId} for user {UserId}", shopId, userId);
        
        try
        {
            await _shopService.DeleteShopAsync(userId.Value, shopId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Shop not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Manually trigger Etsy listing sync.
    /// </summary>
    [HttpPost("{shopId}/sync")]
    [ProducesResponseType(typeof(SyncResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sync(Guid shopId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        _logger.LogInformation("Initiating sync for shop {ShopId} by user {UserId}", shopId, userId);
        
        try
        {
            var result = await _shopService.InitiateSyncAsync(userId.Value, shopId);
            
            return Ok(new SyncResponseDto
            {
                JobId = result.JobId,
                Status = result.Status
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Shop not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (EtsyTokenExpiredException ex)
        {
            _logger.LogWarning(ex, "Token expired during sync for shop {ShopId}", shopId);
            return BadRequest(new { error = "Etsy token expired. Please reconnect your shop." });
        }
        catch (EtsyRateLimitException ex)
        {
            _logger.LogWarning(ex, "Rate limit exceeded during sync for shop {ShopId}", shopId);
            return StatusCode(429, new { error = "Etsy API rate limit exceeded. Please try again later." });
        }
    }

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}

// Request/Response DTOs
public class ShopsResponse
{
    public List<ShopDto> Shops { get; set; } = new();
}

public class ShopDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
}

public class ConnectEtsyRequest
{
    public string? ReturnUrl { get; set; }
}

public class ConnectResponseDto
{
    public string AuthUrl { get; set; } = string.Empty;
}

public class EtsyCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public class CallbackResponseDto
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public bool Connected { get; set; }
}

public class SyncResponseDto
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
