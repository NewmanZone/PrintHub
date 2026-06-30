using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PrintHub.Core.Interfaces.Services;

namespace PrintHub.Infrastructure.Services.Etsy;

/// <summary>
/// Fake implementation of Etsy service for local development and testing.
/// </summary>
public class FakeEtsyService : IEtsyService
{
    private readonly ILogger<FakeEtsyService> _logger;
    private readonly Dictionary<string, string> _fakeTokens = new();
    private readonly Dictionary<string, List<FakeEtsyListing>> _fakeListings = new();

    public FakeEtsyService(ILogger<FakeEtsyService> logger)
    {
        _logger = logger;
    }

    public Task<string> GetAuthorizationUrlAsync(string state, string redirectUri, string? codeChallenge = null)
    {
        _logger.LogInformation("Fake Etsy: Generating authorization URL with state {State}", state);
        
        var fakeAuthUrl = $"https://www.etsy.com/oauth2/authorize" +
            $"?response_type=code" +
            $"&client_id=fake_client_id" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope=listings_r:w%20shop_r" +
            $"&state={state}";
        
        if (!string.IsNullOrEmpty(codeChallenge))
        {
            fakeAuthUrl +=
                $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
                "&code_challenge_method=S256";
        }
        
        return Task.FromResult(fakeAuthUrl);
    }

    public Task<EtsyTokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, string? codeVerifier = null)
    {
        _logger.LogInformation("Fake Etsy: Exchanging code {Code} for token", code);
        
        var accessToken = $"fake_access_token_{Guid.NewGuid():N}";
        var refreshToken = $"fake_refresh_token_{Guid.NewGuid():N}";
        
        _fakeTokens[accessToken] = code;
        
        return Task.FromResult(new EtsyTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    public Task<EtsyTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Fake Etsy: Refreshing token");
        
        var newAccessToken = $"fake_access_token_{Guid.NewGuid():N}";
        var newRefreshToken = $"fake_refresh_token_{Guid.NewGuid():N}";
        
        return Task.FromResult(new EtsyTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600,
            TokenType = "Bearer"
        });
    }

    public Task<EtsyShopInfo> GetShopInfoAsync(string accessToken)
    {
        _logger.LogInformation("Fake Etsy: Getting shop info for token");
        
        return Task.FromResult(new EtsyShopInfo
        {
            ShopId = $"etsy_shop_{accessToken[..8]}",
            ShopName = "Mikes3DPrints",
            Email = "seller@fake-etsy-test.com",
            ImageUrl = "https://i.etsystatic.com/placeholder_shop.png"
        });
    }

    public Task<IEnumerable<EtsyListing>> GetListingsAsync(string accessToken, string shopId)
    {
        _logger.LogInformation("Fake Etsy: Getting listings for shop {ShopId}", shopId);
        
        if (!_fakeListings.ContainsKey(shopId))
        {
            // Generate fake listings for testing
            _fakeListings[shopId] = GenerateFakeListings();
        }
        
        return Task.FromResult<IEnumerable<EtsyListing>>(_fakeListings[shopId].Select(l => (EtsyListing)new EtsyListing
        {
            ListingId = l.ListingId,
            Title = l.Title,
            Description = l.Description,
            Price = l.Price,
            ImageUrl = l.ImageUrl,
            MainImageUrl = l.ImageUrl,
            ImageUrls = string.IsNullOrWhiteSpace(l.ImageUrl) ? new List<string>() : new List<string> { l.ImageUrl },
            IsActive = l.IsActive,
            CreatedAt = l.CreatedAt ?? DateTime.UtcNow,
            UpdatedAt = l.UpdatedAt ?? DateTime.UtcNow,
            State = l.State ?? string.Empty
        }));
    }

    public Task<bool> ValidateTokenAsync(string accessToken)
    {
        var isValid = _fakeTokens.ContainsKey(accessToken) || accessToken.StartsWith("fake_access_token");
        _logger.LogInformation("Fake Etsy: Token validation result: {IsValid}", isValid);
        return Task.FromResult(isValid);
    }

    private List<FakeEtsyListing> GenerateFakeListings()
    {
        return new List<FakeEtsyListing>
        {
            new()
            {
                ListingId = "etsy_listing_001",
                Title = "Dino Wall Hook",
                Description = "Adorable dinosaur wall hook for kids room. Printed in eco-friendly PLA.",
                Price = 24.99m,
                ImageUrl = "https://i.etsystatic.com/placeholder/dino_wall_hook.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                State = "active"
            },
            new()
            {
                ListingId = "etsy_listing_002",
                Title = "Cat Wall Hook",
                Description = "Cute cat face wall hook. Perfect for entryway or bathroom.",
                Price = 22.99m,
                ImageUrl = "https://i.etsystatic.com/placeholder/cat_wall_hook.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                UpdatedAt = DateTime.UtcNow.AddDays(-10),
                State = "active"
            },
            new()
            {
                ListingId = "etsy_listing_003",
                Title = "Bear Wall Hook",
                Description = "Adorable bear face wall hook for children's rooms.",
                Price = 22.99m,
                ImageUrl = "https://i.etsystatic.com/placeholder/bear_wall_hook.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                State = "active"
            },
            new()
            {
                ListingId = "etsy_listing_004",
                Title = "Dragon Keychain",
                Description = "Mini dragon keychain - perfect gift for fantasy lovers!",
                Price = 14.99m,
                ImageUrl = "https://i.etsystatic.com/placeholder/dragon_keychain.jpg",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                UpdatedAt = DateTime.UtcNow.AddDays(-15),
                State = "active"
            },
            new()
            {
                ListingId = "etsy_listing_005",
                Title = "Heart Keychain",
                Description = "Personalized heart keychain with custom text.",
                Price = 12.99m,
                ImageUrl = "https://i.etsystatic.com/placeholder/heart_keychain.jpg",
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddDays(-120),
                UpdatedAt = DateTime.UtcNow.AddDays(-60),
                State = "inactive"
            }
        };
    }
}

internal class FakeEtsyListing
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
