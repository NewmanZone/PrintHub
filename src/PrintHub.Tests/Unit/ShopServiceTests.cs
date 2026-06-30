using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Services.Etsy;
using Xunit;

namespace PrintHub.Tests.Unit;

[Collection("Unit Tests")]
public class ShopServiceTests
{
    private readonly Mock<IShopRepository> _shopRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IEtsyService> _etsyServiceMock;
    private readonly Mock<ITokenEncryptionService> _encryptionMock;
    private readonly Mock<IOAuthStateStore> _stateStoreMock;
    private readonly Mock<ILogger<ShopService>> _loggerMock;
    private readonly EtsyConfiguration _etsyConfig;
    private readonly ShopService _shopService;

    public ShopServiceTests()
    {
        _shopRepoMock = new Mock<IShopRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _etsyServiceMock = new Mock<IEtsyService>();
        _encryptionMock = new Mock<ITokenEncryptionService>();
        _stateStoreMock = new Mock<IOAuthStateStore>();
        _loggerMock = new Mock<ILogger<ShopService>>();
        _etsyConfig = new EtsyConfiguration
        {
            RedirectUri = "http://localhost/callback"
        };

        _shopService = new ShopService(
            _shopRepoMock.Object,
            _productRepoMock.Object,
            _etsyServiceMock.Object,
            _encryptionMock.Object,
            _stateStoreMock.Object,
            _etsyConfig,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetShopsAsync_ReturnsUserShops()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shops = new List<Shop>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, Provider = "etsy", ShopName = "Test Shop" }
        };
        _shopRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(shops);

        // Act
        var result = await _shopService.GetShopsAsync(userId);

        // Assert
        result.Should().HaveCount(1);
        result.First().ShopName.Should().Be("Test Shop");
    }

    [Fact]
    public async Task InitiateEtsyConnectAsync_ReturnsAuthUrl()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUrl = "https://www.etsy.com/oauth2/authorize?...";
        _etsyServiceMock.Setup(s => s.GetAuthorizationUrlAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedUrl);

        // Act
        var result = await _shopService.InitiateEtsyConnectAsync(userId);

        // Assert
        result.AuthUrl.Should().Be(expectedUrl);
        _stateStoreMock.Verify(s => s.SaveState(It.IsAny<string>(), userId.ToString(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HandleEtsyCallbackAsync_InvalidState_ThrowsException()
    {
        // Arrange
        _stateStoreMock.Setup(s => s.GetState(It.IsAny<string>())).Returns((null, null, null));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _shopService.HandleEtsyCallbackAsync("code", "invalid_state"));
    }

    [Fact]
    public async Task HandleEtsyCallbackAsync_ValidState_CreatesShop()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var state = Guid.NewGuid().ToString("N");
        _stateStoreMock.Setup(s => s.GetState(state)).Returns((userId.ToString(), "", "verifier123"));

        var tokenResponse = new EtsyTokenResponse
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresIn = 3600
        };
        _etsyServiceMock.Setup(s => s.ExchangeCodeForTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(tokenResponse);

        var shopInfo = new EtsyShopInfo
        {
            ShopId = "etsy_shop_123",
            ShopName = "Mikes3DPrints"
        };
        _etsyServiceMock.Setup(s => s.GetShopInfoAsync(tokenResponse.AccessToken))
            .ReturnsAsync(shopInfo);

        _encryptionMock.Setup(e => e.Encrypt(It.IsAny<string>())).Returns("encrypted_token");
        _shopRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new List<Shop>());

        // Act
        var result = await _shopService.HandleEtsyCallbackAsync("code", state);

        // Assert
        result.Connected.Should().BeTrue();
        result.ShopName.Should().Be("Mikes3DPrints");
        _shopRepoMock.Verify(r => r.AddAsync(It.Is<Shop>(s => 
            s.UserId == userId && s.ExternalId == "etsy_shop_123")), Times.Once);
    }

    [Fact]
    public async Task DeleteShopAsync_OwnershipEnforcement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop { Id = shopId, UserId = Guid.NewGuid() }; // Different owner
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _shopService.DeleteShopAsync(userId, shopId));
    }

    [Fact]
    public async Task DeleteShopAsync_ValidOwner_DeletesShop()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop { Id = shopId, UserId = userId };
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        // Act
        await _shopService.DeleteShopAsync(userId, shopId);

        // Assert
        _shopRepoMock.Verify(r => r.DeleteAsync(shopId), Times.Once);
    }

    [Fact]
    public async Task InitiateSyncAsync_ImportNewListings()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            ExternalId = "etsy_shop_123",
            AccessToken = "encrypted_access",
            RefreshToken = "encrypted_refresh"
        };
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        _encryptionMock.Setup(e => e.Decrypt("encrypted_access")).Returns("access_token");
        _encryptionMock.Setup(e => e.Decrypt("encrypted_refresh")).Returns("refresh_token");
        _etsyServiceMock.Setup(s => s.ValidateTokenAsync("access_token")).ReturnsAsync(true);

        var listings = new List<EtsyListing>
        {
            new() { ListingId = "listing_1", Title = "Product 1", Price = 19.99m, IsActive = true },
            new() { ListingId = "listing_2", Title = "Product 2", Price = 29.99m, IsActive = true }
        };
        _etsyServiceMock.Setup(s => s.GetListingsAsync("access_token", "etsy_shop_123")).ReturnsAsync(listings);

        _productRepoMock.Setup(r => r.GetByExternalListingIdAsync(It.IsAny<string>(), shopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _shopService.InitiateSyncAsync(userId, shopId);

        // Assert
        result.Status.Should().Be("Accepted");
        _productRepoMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == "Product 1")), Times.Once);
        _productRepoMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.Name == "Product 2")), Times.Once);
    }

    [Fact]
    public async Task InitiateSyncAsync_IdempotentReSync()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            ExternalId = "etsy_shop_123",
            AccessToken = "encrypted_access",
            RefreshToken = "encrypted_refresh"
        };
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        _encryptionMock.Setup(e => e.Decrypt("encrypted_access")).Returns("access_token");
        _encryptionMock.Setup(e => e.Decrypt("encrypted_refresh")).Returns("refresh_token");
        _etsyServiceMock.Setup(s => s.ValidateTokenAsync("access_token")).ReturnsAsync(true);

        var listings = new List<EtsyListing>
        {
            new() { ListingId = "listing_1", Title = "Updated Product 1", Price = 24.99m, IsActive = true }
        };
        _etsyServiceMock.Setup(s => s.GetListingsAsync("access_token", "etsy_shop_123")).ReturnsAsync(listings);

        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            ShopId = shopId,
            ExternalListingId = "listing_1",
            Name = "Old Product 1",
            Price = 19.99m
        };
        _productRepoMock.Setup(r => r.GetByExternalListingIdAsync("listing_1", shopId))
            .ReturnsAsync(existingProduct);

        // Act
        var result = await _shopService.InitiateSyncAsync(userId, shopId);

        // Assert
        result.Status.Should().Be("Accepted");
        _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => 
            p.Name == "Updated Product 1" && p.EtsyPrice == 24.99m)), Times.Once);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task InitiateSyncAsync_ExpiredToken_RefreshesAndRetries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            ExternalId = "etsy_shop_123",
            AccessToken = "encrypted_access",
            RefreshToken = "encrypted_refresh"
        };
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        _encryptionMock.Setup(e => e.Decrypt("encrypted_access")).Returns("old_access_token");
        _encryptionMock.Setup(e => e.Decrypt("encrypted_refresh")).Returns("refresh_token");

        _etsyServiceMock.Setup(s => s.ValidateTokenAsync("old_access_token")).ReturnsAsync(false);

        var newTokens = new EtsyTokenResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpiresIn = 3600
        };
        _etsyServiceMock.Setup(s => s.RefreshTokenAsync("refresh_token")).ReturnsAsync(newTokens);

        _encryptionMock.Setup(e => e.Encrypt("new_access_token")).Returns("new_encrypted_access");
        _encryptionMock.Setup(e => e.Encrypt("new_refresh_token")).Returns("new_encrypted_refresh");

        var listings = new List<EtsyListing>
        {
            new() { ListingId = "listing_1", Title = "Product 1", Price = 19.99m, IsActive = true }
        };
        _etsyServiceMock.Setup(s => s.GetListingsAsync("new_access_token", "etsy_shop_123")).ReturnsAsync(listings);
        _productRepoMock.Setup(r => r.GetByExternalListingIdAsync(It.IsAny<string>(), shopId)).ReturnsAsync((Product?)null);

        // Act
        var result = await _shopService.InitiateSyncAsync(userId, shopId);

        // Assert
        result.Status.Should().Be("Accepted");
        _shopRepoMock.Verify(r => r.UpdateAsync(It.Is<Shop>(s => 
            s.AccessToken == "new_encrypted_access")), Times.Once);
    }

    [Fact]
    public async Task InitiateSyncAsync_TokenExpiredException_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            ExternalId = "etsy_shop_123",
            AccessToken = "encrypted_access",
            RefreshToken = "encrypted_refresh"
        };
        _shopRepoMock.Setup(r => r.GetByIdAsync(shopId)).ReturnsAsync(shop);

        _encryptionMock.Setup(e => e.Decrypt("encrypted_access")).Returns("expired_token");
        _encryptionMock.Setup(e => e.Decrypt("encrypted_refresh")).Returns("expired_refresh");

        _etsyServiceMock.Setup(s => s.ValidateTokenAsync("expired_token")).ReturnsAsync(false);
        _etsyServiceMock.Setup(s => s.RefreshTokenAsync("expired_refresh"))
            .ThrowsAsync(new EtsyTokenExpiredException("Token expired"));

        // Act & Assert
        await Assert.ThrowsAsync<EtsyTokenExpiredException>(() => 
            _shopService.InitiateSyncAsync(userId, shopId));
    }
    [Fact]
    public async Task InitiateEtsyConnectAsync_GeneratesPkceAndChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var returnUrl = "/shops";
        var redirectUri = "https://localhost/callback";
        var fakeAuthUrl = "https://www.etsy.com/oauth2/authorize?state=abc";

        _etsyConfigMock.Setup(c => c.RedirectUri).Returns(redirectUri);

        string? capturedVerifier = null;
        string? capturedChallenge = null;
        _etsyServiceMock
            .Setup(s => s.GetAuthorizationUrlAsync(It.IsAny<string>(), redirectUri, It.IsAny<string?>()))
            .ReturnsAsync((string state, string _, string? challenge) =>
            {
                capturedChallenge = challenge;
                return fakeAuthUrl;
            });

        _stateStoreMock
            .Setup(s => s.SaveState(It.IsAny<string>(), userId.ToString(), returnUrl, It.IsAny<TimeSpan>(), It.IsAny<string?>()))
            .Callback((string _, string __, string ___, TimeSpan ____, string? verifier) => capturedVerifier = verifier);

        // Act
        var result = await _shopService.InitiateEtsyConnectAsync(userId, returnUrl);

        // Assert
        result.Should().NotBeNull();
        result.AuthorizationUrl.Should().Be(fakeAuthUrl);
        capturedVerifier.Should().NotBeNullOrEmpty();
        capturedVerifier.Length.Should().BeGreaterThanOrEqualTo(43);
        capturedChallenge.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task HandleEtsyCallbackAsync_UsesPkceVerifier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var state = "valid_state";
        var code = "auth_code";
        var redirectUri = "https://localhost/callback";
        var verifier = "verifier123";

        _stateStoreMock.Setup(s => s.GetState(state)).Returns((userId.ToString(), "/shops", verifier));
        _etsyConfigMock.Setup(c => c.RedirectUri).Returns(redirectUri);

        var tokenResponse = new EtsyTokenResponse
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _etsyServiceMock
            .Setup(s => s.ExchangeCodeForTokenAsync(code, redirectUri, verifier))
            .ReturnsAsync(tokenResponse);

        _encryptionMock.Setup(e => e.Encrypt("access_token")).Returns("enc_access");
        _encryptionMock.Setup(e => e.Encrypt("refresh_token")).Returns("enc_refresh");

        var shopInfo = new EtsyShopInfo { ShopId = "etsy_shop_123", ShopName = "My Shop" };
        _etsyServiceMock.Setup(s => s.GetShopInfoAsync("access_token")).ReturnsAsync(shopInfo);

        // Act
        var result = await _shopService.HandleEtsyCallbackAsync(code, state);

        // Assert
        result.Should().NotBeNull();
        _etsyServiceMock.Verify(s => s.ExchangeCodeForTokenAsync(code, redirectUri, verifier), Times.Once);
    }

    [Fact]
    public async Task RefreshExpiredTokenAsync_RefreshesAndUpdatesShop()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "refresh_token";

        var newTokenResponse = new EtsyTokenResponse
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _etsyServiceMock.Setup(s => s.RefreshTokenAsync(refreshToken)).ReturnsAsync(newTokenResponse);
        _encryptionMock.Setup(e => e.Encrypt("new_access_token")).Returns("new_encrypted_access");
        _encryptionMock.Setup(e => e.Encrypt("new_refresh_token")).Returns("new_encrypted_refresh");

        // Act
        var result = await _shopService.RefreshExpiredTokenAsync(userId, refreshToken);

        // Assert
        result.AccessToken.Should().Be("new_encrypted_access");
        result.RefreshToken.Should().Be("new_encrypted_refresh");
        _shopRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Shop>()), Times.Once);
    }
}
