using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Infrastructure.Services.Etsy;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("workspaces/{workspaceId:guid}/shops")]
[Authorize]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;
    private readonly ILogger<ShopsController> _logger;
    private readonly IWorkspaceAuthorizationService? _authorization;

    public ShopsController(IShopService shopService, ILogger<ShopsController> logger)
    {
        _shopService = shopService;
        _logger = logger;
    }

    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public ShopsController(IShopService shopService, IWorkspaceAuthorizationService authorization, ILogger<ShopsController> logger)
        : this(shopService, logger) => _authorization = authorization;

    [HttpGet]
    public async Task<IActionResult> GetWorkspaceShops(Guid workspaceId)
    {
        if (!await IsMember(workspaceId)) return Forbid();
        return Ok(new ShopsResponse { Shops = (await _shopService.GetShopsForWorkspaceAsync(workspaceId)).Select(ToDto).ToList() });
    }

    [HttpPost("connect/etsy")]
    public async Task<IActionResult> ConnectWorkspaceEtsy(Guid workspaceId, [FromBody] ConnectEtsyRequest? request = null)
    {
        if (!await IsOwner(workspaceId)) return Forbid();
        var result = await _shopService.InitiateWorkspaceEtsyConnectAsync(workspaceId, GetUserId()!.Value, request?.ReturnUrl);
        return Ok(new ConnectResponseDto { AuthUrl = result.AuthUrl });
    }

    [HttpPost("etsy/callback")]
    public async Task<IActionResult> WorkspaceEtsyCallback(Guid workspaceId, [FromBody] EtsyCallbackRequest request)
    {
        if (!await IsOwner(workspaceId)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.State)) return BadRequest();
        try
        {
            var result = await _shopService.HandleWorkspaceEtsyCallbackAsync(workspaceId, GetUserId()!.Value, request.Code, request.State);
            return Ok(new CallbackResponseDto { ShopId = result.ShopId, ShopName = result.ShopName, Connected = result.Connected });
        }
        catch (InvalidOperationException) { return BadRequest(new { error = "Invalid or expired OAuth state" }); }
    }

    [HttpPost("{shopId:guid}/sync")]
    public async Task<IActionResult> SyncWorkspaceShop(Guid workspaceId, Guid shopId)
    {
        if (!await IsContributor(workspaceId)) return Forbid();
        try
        {
            var result = await _shopService.SyncWorkspaceShopAsync(workspaceId, shopId);
            return Ok(new SyncResponseDto { JobId = result.JobId, Status = result.Status });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{shopId:guid}")]
    public async Task<IActionResult> DisconnectWorkspaceShop(Guid workspaceId, Guid shopId)
    {
        if (!await IsOwner(workspaceId)) return Forbid();
        try { await _shopService.DeleteWorkspaceShopAsync(workspaceId, shopId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    private Task<bool> IsMember(Guid id) => _authorization?.IsMemberAsync(id) ?? Task.FromResult(false);
    private Task<bool> IsContributor(Guid id) => _authorization?.IsInRoleAsync(id, PrintHub.Core.Enums.WorkspaceRole.Contributor) ?? Task.FromResult(false);
    private Task<bool> IsOwner(Guid id) => _authorization?.IsOwnerAsync(id) ?? Task.FromResult(false);
    private static ShopDto ToDto(ShopResponse s) => new() { Id = s.Id, Provider = s.Provider, ExternalId = s.ExternalId, ShopName = s.ShopName, IsActive = s.IsActive, LastSyncAt = s.LastSyncAt };

    /// <summary>
    /// List all connected shops for the authenticated user.
    /// </summary>
    [NonAction]
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
    [NonAction]
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
    /// Etsy OAuth callback (Etsy redirects here via GET with query params).
    /// </summary>
    [NonAction]
    [ProducesResponseType(typeof(CallbackResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EtsyCallbackGet([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return BadRequest(new { error = "Code and state query parameters are required" });
        }
        
        _logger.LogInformation("Handling Etsy GET callback with state {State}", state);
        
        try
        {
            var result = await _shopService.HandleEtsyCallbackAsync(code, state);
            
            return Ok(new CallbackResponseDto
            {
                ShopId = result.ShopId,
                ShopName = result.ShopName,
                Connected = result.Connected
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid callback state {State}", state);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Etsy OAuth callback (handled by frontend redirect).
    /// </summary>
    [NonAction]
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
    [NonAction]
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
    [NonAction]
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
