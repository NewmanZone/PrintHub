using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Service for managing shops and Etsy connections.
/// </summary>
public interface IShopService
{
    Task<IEnumerable<ShopResponse>> GetShopsForWorkspaceAsync(Guid workspaceId);
    Task<ConnectResponse> InitiateWorkspaceEtsyConnectAsync(Guid workspaceId, Guid userId, string? returnUrl = null);
    Task<CallbackResponse> HandleWorkspaceEtsyCallbackAsync(Guid workspaceId, Guid userId, string code, string state);
    Task DeleteWorkspaceShopAsync(Guid workspaceId, Guid shopId);
    Task<SyncResponse> SyncWorkspaceShopAsync(Guid workspaceId, Guid shopId);
    Task<IEnumerable<ShopResponse>> GetShopsAsync(Guid userId);
    Task<ConnectResponse> InitiateEtsyConnectAsync(Guid userId, string? returnUrl = null);
    Task<CallbackResponse> HandleEtsyCallbackAsync(string code, string state);
    Task DeleteShopAsync(Guid userId, Guid shopId);
    Task<SyncResponse> InitiateSyncAsync(Guid userId, Guid shopId);
}

/// <summary>
/// Response for shop list endpoint.
/// </summary>
public class ShopResponse
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastSyncAt { get; set; }
}

/// <summary>
/// Response for initiate connect endpoint.
/// </summary>
public class ConnectResponse
{
    public string AuthUrl { get; set; } = string.Empty;
}

/// <summary>
/// Response for OAuth callback endpoint.
/// </summary>
public class CallbackResponse
{
    public Guid ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public bool Connected { get; set; }
}

/// <summary>
/// Response for sync endpoint.
/// </summary>
public class SyncResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
