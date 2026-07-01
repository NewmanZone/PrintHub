using System.Net.Http.Headers;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PrintHub.Core.Interfaces.Services;

namespace PrintHub.Infrastructure.Services.Etsy;

/// <summary>
/// Production implementation of Etsy API service using OAuth2.
/// </summary>
public class EtsyApiService : IEtsyService
{
    private readonly HttpClient _httpClient;
    private readonly EtsyConfiguration _config;
    private readonly ILogger<EtsyApiService> _logger;

    public EtsyApiService(
        HttpClient httpClient,
        EtsyConfiguration config,
        ILogger<EtsyApiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri, string? codeChallenge = null)
    {
        var url = $"{_config.AuthorizationUrl}" +
            $"?response_type=code" +
            $"&client_id={_config.ClientId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(_config.Scopes)}" +
            $"&state={state}";
        
        if (!string.IsNullOrEmpty(codeChallenge))
        {
            url += $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                   "&code_challenge_method=S256";
        }
        
        _logger.LogInformation("Etsy: Generating authorization URL");
        return Task.FromResult(url);
    }

    public async Task<EtsyTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, string? codeVerifier = null)
    {
        _logger.LogInformation("Etsy: Exchanging authorization code for tokens");

        Dictionary<string, string> body;
        bool useBasicAuth;
        if (!string.IsNullOrEmpty(codeVerifier))
        {
            // PKCE public-client token exchange: no client_secret, no Basic auth.
            body = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", _config.ClientId },
                { "code_verifier", codeVerifier }
            };
            useBasicAuth = false;
        }
        else
        {
            // Legacy confidential-client token exchange.
            body = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", _config.ClientId },
                { "client_secret", _config.ClientSecret }
            };
            useBasicAuth = true;
        }

        var response = await SendTokenRequestAsync(body, useBasicAuth);
        return response;
    }

    public async Task<EtsyTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Etsy: Refreshing access token");
        
        var body = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken },
            { "client_id", _config.ClientId },
            { "client_secret", _config.ClientSecret }
        };

        var response = await SendTokenRequestAsync(body);
        return response;
    }

    private async Task<EtsyTokenResponse> SendTokenRequestAsync(Dictionary<string, string> body, bool useBasicAuth = true)
    {
        var content = new FormUrlEncodedContent(body);
        
        var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenUrl)
        {
            Content = content
        };
        
        // Add Basic auth header for confidential clients only.
        if (useBasicAuth)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<EtsyTokenJson>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize Etsy token response");
        
        return new EtsyTokenResponse
        {
            AccessToken = tokenData.AccessToken,
            RefreshToken = tokenData.RefreshToken ?? string.Empty,
            ExpiresIn = tokenData.ExpiresIn,
            TokenType = tokenData.TokenType ?? "Bearer"
        };
    }

    public async Task<EtsyShopInfo> GetShopInfoAsync(string accessToken)
    {
        _logger.LogInformation("Etsy: Getting shop info");
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_config.BaseUrl}/users/__SELF__/shops");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("x-api-key", _config.ClientId);
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new EtsyTokenExpiredException("Etsy access token has expired");
        }
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var shopResponse = JsonSerializer.Deserialize<EtsyShopResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize Etsy shop response");
        
        var shopData = shopResponse.Results.FirstOrDefault() ?? throw new InvalidOperationException("No shops found in Etsy response");
        
        return new EtsyShopInfo
        {
            ShopId = shopData.ShopId.ToString(),
            ShopName = shopData.ShopName ?? string.Empty,
            Email = shopData.Email,
            ImageUrl = shopData.ImageUrl
        };
    }

    public async Task<IEnumerable<EtsyListing>> GetListingsAsync(string accessToken, string shopId)
    {
        _logger.LogInformation("Etsy: Getting listings for shop {ShopId}", shopId);
        
        var listings = new List<EtsyListing>();
        int offset = 0;
        const int limit = 50;
        
        while (true)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{_config.BaseUrl}/shops/{shopId}/listings/active?limit=100&offset={offset}&includes=Images");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Add("x-api-key", _config.ClientId);
            
            var response = await _httpClient.SendAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new EtsyTokenExpiredException("Etsy access token has expired");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new EtsyRateLimitException("Etsy API rate limit exceeded");
            }
            
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var listingData = JsonSerializer.Deserialize<EtsyListingResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new InvalidOperationException("Failed to deserialize Etsy listing response");
            
            foreach (var item in listingData.Results)
            {
                var imageUrl = item.MainImage?.UrlFull ?? item.Images?.FirstOrDefault()?.UrlFull;
                listings.Add(new EtsyListing
                {
                    ListingId = item.ListingId.ToString(),
                    Title = item.Title ?? string.Empty,
                    Description = item.Description,
                    Price = item.Price ?? 0,
                    ImageUrl = imageUrl,
                    MainImageUrl = imageUrl,
                    ImageUrls = item.Images?.Select(i => i.UrlFull).Where(url => !string.IsNullOrWhiteSpace(url)).Cast<string>().ToList() ?? new List<string>(),
                    IsActive = item.State == "active",
                    CreatedAt = item.CreationDate ?? DateTime.UtcNow,
                    UpdatedAt = item.LastModifiedDate ?? DateTime.UtcNow,
                    State = item.State ?? string.Empty
                });
            }
            
            if (!listingData.Pagination?.HasMorePage ?? true)
                break;
            
            offset += limit;
        }
        
        return listings;
    }

    public async Task<bool> ValidateTokenAsync(string accessToken)
    {
        try
        {
            await GetShopInfoAsync(accessToken);
            return true;
        }
        catch (EtsyTokenExpiredException)
        {
            return false;
        }
    }
}

// JSON DTOs
internal class EtsyTokenJson
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string? TokenType { get; set; }
}

internal class EtsyShopResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("results")]
    public List<EtsyShopJson> Results { get; set; } = new();
}

internal class EtsyShopJson
{
    [System.Text.Json.Serialization.JsonPropertyName("shop_id")]
    public long ShopId { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("shop_name")]
    public string? ShopName { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
}

internal class EtsyListingResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("results")]
    public List<EtsyListingItem> Results { get; set; } = new();
    
    [System.Text.Json.Serialization.JsonPropertyName("pagination")]
    public EtsyPagination? Pagination { get; set; }
}

internal class EtsyListingItem
{
    [System.Text.Json.Serialization.JsonPropertyName("listing_id")]
    public long ListingId { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("price")]
    public decimal? Price { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("state")]
    public string? State { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("creation_date")]
    public DateTime? CreationDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("last_modified_date")]
    public DateTime? LastModifiedDate { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("main_image")]
    public EtsyImageInfo? MainImage { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("images")]
    public List<EtsyImageInfo>? Images { get; set; }
}

internal class EtsyImageInfo
{
    [System.Text.Json.Serialization.JsonPropertyName("url_full")]
    public string? UrlFull { get; set; }
}

internal class EtsyPagination
{
    public bool? HasMorePage { get; set; }
}

/// <summary>
/// Exception thrown when Etsy token has expired.
/// </summary>
public class EtsyTokenExpiredException : Exception
{
    public EtsyTokenExpiredException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when Etsy API rate limit is exceeded.
/// </summary>
public class EtsyRateLimitException : Exception
{
    public EtsyRateLimitException(string message) : base(message) { }
}

/// <summary>
/// Exception thrown when Etsy API is unavailable.
/// </summary>
public class EtsyApiException : Exception
{
    public EtsyApiException(string message) : base(message) { }
}
