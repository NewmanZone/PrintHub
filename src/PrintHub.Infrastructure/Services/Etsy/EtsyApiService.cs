using System.Net.Http.Headers;
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

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri)
    {
        var url = $"{_config.AuthorizationUrl}" +
            $"?response_type=code" +
            $"&client_id={_config.ClientId}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope={Uri.EscapeDataString(_config.Scopes)}" +
            $"&state={state}";
        
        _logger.LogInformation("Etsy: Generating authorization URL");
        return Task.FromResult(url);
    }

    public async Task<EtsyTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri)
    {
        _logger.LogInformation("Etsy: Exchanging authorization code for tokens");
        
        var body = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "client_id", _config.ClientId },
            { "client_secret", _config.ClientSecret }
        };

        var response = await SendTokenRequestAsync(body);
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

    private async Task<EtsyTokenResponse> SendTokenRequestAsync(Dictionary<string, string> body)
    {
        var content = new FormUrlEncodedContent(body);
        
        var request = new HttpRequestMessage(HttpMethod.Post, _config.TokenUrl)
        {
            Content = content
        };
        
        // Add Basic auth header (client_id:client_secret)
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        
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
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_config.BaseUrl}/v3/application/shop");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            throw new EtsyTokenExpiredException("Etsy access token has expired");
        }
        
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var shopData = JsonSerializer.Deserialize<EtsyShopJson>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize Etsy shop response");
        
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
                $"{_config.BaseUrl}/v3/application/shop/{shopId}/listings/active?offset={offset}&limit={limit}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            
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
                listings.Add(new EtsyListing
                {
                    ListingId = item.ListingId.ToString(),
                    Title = item.Title ?? string.Empty,
                    Description = item.Description,
                    Price = item.Price ?? 0,
                    ImageUrl = item.MainImage?.UrlFull ?? item.Images?.FirstOrDefault()?.UrlFull,
                    IsActive = item.State == "active",
                    CreatedAt = item.CreationDate,
                    UpdatedAt = item.LastModifiedDate,
                    State = item.State
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
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? TokenType { get; set; }
}

internal class EtsyShopJson
{
    public long ShopId { get; set; }
    public string? ShopName { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
}

internal class EtsyListingResponse
{
    public List<EtsyListingItem> Results { get; set; } = new();
    public EtsyPagination? Pagination { get; set; }
}

internal class EtsyListingItem
{
    public long ListingId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? State { get; set; }
    public DateTime? CreationDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public EtsyImageInfo? MainImage { get; set; }
    public List<EtsyImageInfo>? Images { get; set; }
}

internal class EtsyImageInfo
{
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