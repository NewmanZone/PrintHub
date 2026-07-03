namespace PrintHub.Infrastructure.Services;

/// <summary>
/// Configuration for Etsy API integration.
/// </summary>
public class EtsyConfiguration
{
    public const string SectionName = "Etsy";
    
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.etsy.com";
    public string AuthorizationUrl { get; set; } = "https://www.etsy.com/oauth2/authorize";
    public string TokenUrl { get; set; } = "https://api.etsy.com/v3/oauth/token";
    public string RedirectUri { get; set; } = string.Empty;
    public string Scopes { get; set; } = "listings_r:w shop_r";
    
    // For fake mode (development)
    public bool UseFakeProvider { get; set; } = false;
}