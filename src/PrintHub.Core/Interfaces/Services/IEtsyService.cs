namespace PrintHub.Core.Interfaces.Services;

/// <summary>
/// Service for interacting with Etsy OAuth and API.
/// </summary>
public interface IEtsyService
{
    /// <summary>
    /// Builds the Etsy OAuth authorization URL for the given state and redirect URI.
    /// </summary>
    Task<string> GetAuthorizationUrlAsync(string state, string redirectUri, string? codeChallenge = null);

    /// <summary>
    /// Exchanges the OAuth authorization code for access/refresh tokens.
    /// </summary>
    Task<EtsyTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, string? codeVerifier = null);

    /// <summary>
    /// Refreshes an expired access token using the refresh token.
    /// </summary>
    Task<EtsyTokenResponse> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Retrieves Etsy shop information for the authenticated user.
    /// </summary>
    Task<EtsyShopInfo> GetShopInfoAsync(string accessToken);

    /// <summary>
    /// Retrieves active listings for the given Etsy shop.
    /// </summary>
    Task<IEnumerable<EtsyListing>> GetListingsAsync(string accessToken, string shopId);
}

/// <summary>
/// Etsy OAuth token response.
/// </summary>
public class EtsyTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? TokenType { get; set; }
}

/// <summary>
/// Etsy shop information.
/// </summary>
public class EtsyShopInfo
{
    public string ShopId { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Etsy listing summary.
/// </summary>
public class EtsyListing
{
    public string ListingId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string State { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? MainImageUrl { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}
