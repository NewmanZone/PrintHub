using PrintHub.Core.Entities;

namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Service for interacting with Etsy API.
/// </summary>
public interface IEtsyService
{
    /// <summary>
    /// Gets the URL to initiate Etsy OAuth flow.
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(string state, string redirectUri);
    
    /// <summary>
    /// Exchanges authorization code for tokens.
    /// </summary>
    Task<EtsyTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri);
    
    /// <summary>
    /// Refreshes an expired access token.
    /// </summary>
    Task<EtsyTokenResponse> RefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Gets shop information from Etsy.
    /// </summary>
    Task<EtsyShopInfo> GetShopInfoAsync(string accessToken);
    
    /// <summary>
    /// Gets all listings for a shop from Etsy.
    /// </summary>
    Task<IEnumerable<EtsyListing>> GetListingsAsync(string accessToken, string shopId);
    
    /// <summary>
    /// Validates that an access token is still valid.
    /// </summary>
    Task<bool> ValidateTokenAsync(string accessToken);
}

/// <summary>
/// Response from Etsy token exchange.
/// </summary>
public class EtsyTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}

/// <summary>
/// Shop information from Etsy.
/// </summary>
public class EtsyShopInfo
{
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Listing from Etsy.
/// </summary>
public class EtsyListing
{
    public string ListingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? State { get; set; }
}