using Microsoft.Extensions.Logging;
using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services.Etsy;

namespace PrintHub.Infrastructure.Services;

/// <summary>
/// Implementation of shop management service with Etsy OAuth integration.
/// </summary>
public class ShopService : IShopService
{
    private readonly IShopRepository _shopRepository;
    private readonly IProductRepository _productRepository;
    private readonly IPartRepository _partRepository;
    private readonly IPrintFileRepository _printFileRepository;
    private readonly IEtsyService _etsyService;
    private readonly ITokenEncryptionService _tokenEncryption;
    private readonly IOAuthStateStore _oauthStateStore;
    private readonly EtsyConfiguration _etsyConfig;
    private readonly ILogger<ShopService> _logger;

    public ShopService(
        IShopRepository shopRepository,
        IProductRepository productRepository,
        IPartRepository partRepository,
        IPrintFileRepository printFileRepository,
        IEtsyService etsyService,
        ITokenEncryptionService tokenEncryption,
        IOAuthStateStore oauthStateStore,
        EtsyConfiguration etsyConfig,
        ILogger<ShopService> logger)
    {
        _shopRepository = shopRepository;
        _productRepository = productRepository;
        _partRepository = partRepository;
        _printFileRepository = printFileRepository;
        _etsyService = etsyService;
        _tokenEncryption = tokenEncryption;
        _oauthStateStore = oauthStateStore;
        _etsyConfig = etsyConfig;
        _logger = logger;
    }

    public async Task<IEnumerable<ShopResponse>> GetShopsAsync(Guid userId)
    {
        var shops = await _shopRepository.GetByUserIdAsync(userId);
        
        return shops.Select(s => new ShopResponse
        {
            Id = s.Id,
            Provider = s.Provider,
            ExternalId = s.ExternalId,
            ShopName = s.ShopName,
            IsActive = s.IsActive,
            LastSyncAt = s.LastSyncAt
        });
    }

    public async Task<IEnumerable<ShopResponse>> GetShopsForWorkspaceAsync(Guid workspaceId)
    {
        var shops = await _shopRepository.GetByWorkspaceIdAsync(workspaceId);
        return shops.Select(ToResponse);
    }

    private static ShopResponse ToResponse(Shop s) => new()
    {
        Id = s.Id, Provider = s.Provider, ExternalId = s.ExternalId, ShopName = s.ShopName,
        IsActive = s.IsActive, LastSyncAt = s.LastSyncAt
    };

    public async Task<ConnectResponse> InitiateWorkspaceEtsyConnectAsync(Guid workspaceId, Guid userId, string? returnUrl = null)
    {
        var state = Guid.NewGuid().ToString("N");
        var codeVerifier = GenerateCodeVerifier();
        _oauthStateStore.SaveState(state, $"{userId:N}|{workspaceId:N}", returnUrl ?? string.Empty,
            TimeSpan.FromMinutes(10), codeVerifier);
        var authUrl = await _etsyService.GetAuthorizationUrlAsync(state, GetWorkspaceRedirectUri(workspaceId), GenerateCodeChallenge(codeVerifier));
        return new ConnectResponse { AuthUrl = authUrl };
    }

    public async Task<CallbackResponse> HandleWorkspaceEtsyCallbackAsync(Guid workspaceId, Guid userId, string code, string state)
    {
        var (context, _, codeVerifier) = _oauthStateStore.GetState(state);
        if (context != $"{userId:N}|{workspaceId:N}")
            throw new InvalidOperationException("Invalid or expired OAuth state");
        _oauthStateStore.DeleteState(state);
        var token = await _etsyService.ExchangeCodeForTokenAsync(code, GetWorkspaceRedirectUri(workspaceId), codeVerifier);
        var info = await _etsyService.GetShopInfoAsync(token.AccessToken);
        var existing = (await _shopRepository.GetByWorkspaceIdAsync(workspaceId)).FirstOrDefault();
        var now = DateTime.UtcNow;
        var shop = new Shop
        {
            Id = existing?.Id ?? Guid.NewGuid(), WorkspaceId = workspaceId, UserId = userId,
            Provider = "etsy", ExternalId = info.ShopId, ShopName = info.ShopName, IsActive = true,
            AccessToken = _tokenEncryption.Encrypt(token.AccessToken),
            RefreshToken = _tokenEncryption.Encrypt(token.RefreshToken ?? string.Empty),
            TokenExpiresAt = now.AddSeconds(token.ExpiresIn), CreatedAt = existing?.CreatedAt ?? now, UpdatedAt = now
        };
        if (existing is null) await _shopRepository.AddAsync(shop); else await _shopRepository.UpdateAsync(shop);
        return new CallbackResponse { ShopId = shop.Id, ShopName = shop.ShopName, Connected = true };
    }

    public async Task DeleteWorkspaceShopAsync(Guid workspaceId, Guid shopId)
    {
        var shop = await GetWorkspaceShop(workspaceId, shopId);
        await DeleteShopContentsAsync(shop);
    }

    public async Task<SyncResponse> SyncWorkspaceShopAsync(Guid workspaceId, Guid shopId)
    {
        await GetWorkspaceShop(workspaceId, shopId);
        return await SyncShopAsync(shopId);
    }

    private async Task<Shop> GetWorkspaceShop(Guid workspaceId, Guid shopId)
    {
        var shop = await _shopRepository.GetByIdAsync(shopId);
        if (shop is null || shop.WorkspaceId != workspaceId) throw new KeyNotFoundException($"Shop {shopId} not found");
        return shop;
    }

    private async Task DeleteShopContentsAsync(Shop shop)
    {
        var products = await _productRepository.GetByShopIdWithPartsAsync(shop.Id);
        foreach (var product in products) await _productRepository.DeleteAsync(product.Id);
        await _shopRepository.DeleteAsync(shop.Id);
    }

    private async Task<SyncResponse> SyncShopAsync(Guid shopId)
    {
        var shop = (await _shopRepository.GetByIdAsync(shopId))!;
        var accessToken = _tokenEncryption.Decrypt(shop.AccessToken);
        if (!await _etsyService.ValidateTokenAsync(accessToken))
        {
            var refreshed = await _etsyService.RefreshTokenAsync(_tokenEncryption.Decrypt(shop.RefreshToken));
            shop.AccessToken = _tokenEncryption.Encrypt(refreshed.AccessToken);
            shop.RefreshToken = _tokenEncryption.Encrypt(refreshed.RefreshToken ?? string.Empty);
            shop.TokenExpiresAt = DateTime.UtcNow.AddSeconds(refreshed.ExpiresIn);
            accessToken = refreshed.AccessToken;
        }
        var imported = 0;
        foreach (var listing in await _etsyService.GetListingsAsync(accessToken, shop.ExternalId))
        {
            var product = await _productRepository.GetByExternalListingIdAsync(listing.ListingId, shop.Id);
            var isNew = product is null;
            if (product is null)
            {
                product = new Product { Id = Guid.NewGuid(), ShopId = shop.Id, ExternalListingId = listing.ListingId, CreatedAt = DateTime.UtcNow };
                imported++;
            }
            product.Name = listing.Title; product.Description = listing.Description; product.EtsyPrice = listing.Price;
            product.ImageUrl = listing.ImageUrl; product.IsActive = listing.IsActive; product.UpdatedAt = DateTime.UtcNow;
            if (isNew) await _productRepository.AddAsync(product); else await _productRepository.UpdateAsync(product);
        }
        shop.LastSyncAt = DateTime.UtcNow;
        await _shopRepository.UpdateAsync(shop);
        return new SyncResponse { JobId = $"sync_{shop.Id:N}_{DateTime.UtcNow.Ticks}", Status = "Completed" };
    }

    private string GetRedirectUri()
    {
        if (!string.IsNullOrEmpty(_etsyConfig.RedirectUri))
            return _etsyConfig.RedirectUri;
        // Derive from ApiBaseUrl when config does not set RedirectUri
        var baseUrl = _etsyConfig.BaseUrl?.TrimEnd('/');
        return !string.IsNullOrEmpty(baseUrl) ? $"{baseUrl}/api/etsy/callback" : string.Empty;
    }

    private string GetWorkspaceRedirectUri(Guid workspaceId)
    {
        if (!string.IsNullOrEmpty(_etsyConfig.RedirectUri)) return _etsyConfig.RedirectUri;
        var baseUrl = _etsyConfig.BaseUrl?.TrimEnd('/');
        return !string.IsNullOrEmpty(baseUrl)
            ? $"{baseUrl}/workspaces/{workspaceId}/shops/etsy/callback"
            : string.Empty;
    }

    public async Task<ConnectResponse> InitiateEtsyConnectAsync(Guid userId, string? returnUrl = null)
    {
        _logger.LogInformation("Initiating Etsy OAuth flow for user {UserId}", userId);
        
        // Generate state and PKCE verifier for OAuth security
        var state = Guid.NewGuid().ToString("N");
        var codeVerifier = GenerateCodeVerifier();
        
        // Store state with user context and PKCE verifier
        _oauthStateStore.SaveState(state, userId.ToString(), returnUrl ?? string.Empty, TimeSpan.FromMinutes(10), codeVerifier);
        
        // Build authorization URL with PKCE challenge
        var redirectUri = GetRedirectUri();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var authUrl = await _etsyService.GetAuthorizationUrlAsync(state, redirectUri, codeChallenge);
        
        return new ConnectResponse { AuthUrl = authUrl };
    }

    public async Task<CallbackResponse> HandleEtsyCallbackAsync(string code, string state)
    {
        _logger.LogInformation("Handling Etsy OAuth callback with state {State}", state);
        
        // Validate state and get user context + PKCE verifier
        var (userIdStr, returnUrl, codeVerifier) = _oauthStateStore.GetState(state);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Invalid or expired OAuth state: {State}", state);
            throw new InvalidOperationException("Invalid or expired OAuth state");
        }
        
        // Clean up state
        _oauthStateStore.DeleteState(state);
        
        // Exchange code for tokens (PKCE)
        var tokenResponse = await _etsyService.ExchangeCodeForTokenAsync(code, GetRedirectUri(), codeVerifier);
        
        // Get shop info from Etsy
        var shopInfo = await _etsyService.GetShopInfoAsync(tokenResponse.AccessToken);
        
        // Check if shop already exists for this user
        var existingShop = await _shopRepository.GetByUserIdAsync(userId)
            .ContinueWith(t => t.Result.FirstOrDefault(s => s.ExternalId == shopInfo.ShopId));
        
        var shopId = existingShop?.Id ?? Guid.NewGuid();
        var isNewShop = existingShop == null;
        
        // Encrypt tokens before storing
        var encryptedAccessToken = _tokenEncryption.Encrypt(tokenResponse.AccessToken);
        var encryptedRefreshToken = _tokenEncryption.Encrypt(tokenResponse.RefreshToken ?? string.Empty);
        
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            Provider = "etsy",
            ExternalId = shopInfo.ShopId,
            AccessToken = encryptedAccessToken,
            RefreshToken = encryptedRefreshToken,
            TokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn),
            ShopName = shopInfo.ShopName,
            IsActive = true,
            CreatedAt = isNewShop ? DateTime.UtcNow : existingShop!.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
        
        if (isNewShop)
        {
            await _shopRepository.AddAsync(shop);
            _logger.LogInformation("Created new shop {ShopId} for user {UserId}", shopId, userId);
        }
        else
        {
            await _shopRepository.UpdateAsync(shop);
            _logger.LogInformation("Updated existing shop {ShopId} for user {UserId}", shopId, userId);
        }
        
        return new CallbackResponse
        {
            ShopId = shopId,
            ShopName = shop.ShopName,
            Connected = true
        };
    }

    public async Task DeleteShopAsync(Guid userId, Guid shopId)
    {
        _logger.LogInformation("Deleting shop {ShopId} for user {UserId}", shopId, userId);
        
        var shop = await _shopRepository.GetByIdAsync(shopId);
        if (shop == null)
        {
            throw new KeyNotFoundException($"Shop {shopId} not found");
        }
        
        // Verify ownership
        if (shop.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to delete shop {ShopId} owned by {OwnerId}", 
                userId, shopId, shop.UserId);
            throw new UnauthorizedAccessException("You do not have permission to delete this shop");
        }
        
        // Clean up associated products before deleting the shop
        var products = await _productRepository.GetByShopIdWithPartsAsync(shopId);
        foreach (var product in products)
        {
            // Delete parts and their associated files for this product
            if (product.ProductParts != null)
            {
                foreach (var productPart in product.ProductParts)
                {
                    if (productPart.Part != null)
                    {
                        var files = await _printFileRepository.GetByPartIdAsync(productPart.Part.Id);
                        foreach (var file in files)
                        {
                            await _printFileRepository.DeleteAsync(file.Id);
                        }
                        await _partRepository.DeleteAsync(productPart.Part.Id);
                    }
                }
            }
            await _productRepository.DeleteAsync(product.Id);
        }
        
        await _shopRepository.DeleteAsync(shopId);
    }

    public async Task<SyncResponse> InitiateSyncAsync(Guid userId, Guid shopId)
    {
        _logger.LogInformation("Initiating sync for shop {ShopId} by user {UserId}", shopId, userId);
        
        var shop = await _shopRepository.GetByIdAsync(shopId);
        if (shop == null)
        {
            throw new KeyNotFoundException($"Shop {shopId} not found");
        }
        
        // Verify ownership
        if (shop.UserId != userId)
        {
            _logger.LogWarning("User {UserId} attempted to sync shop {ShopId} owned by {OwnerId}", 
                userId, shopId, shop.UserId);
            throw new UnauthorizedAccessException("You do not have permission to sync this shop");
        }
        
        // Decrypt access token
        var accessToken = _tokenEncryption.Decrypt(shop.AccessToken);
        
        // Validate token first
        var isTokenValid = await _etsyService.ValidateTokenAsync(accessToken);
        if (!isTokenValid)
        {
            // Try to refresh
            var refreshToken = _tokenEncryption.Decrypt(shop.RefreshToken);
            var newTokens = await _etsyService.RefreshTokenAsync(refreshToken);
            
            // Update encrypted tokens
            shop.AccessToken = _tokenEncryption.Encrypt(newTokens.AccessToken);
            shop.RefreshToken = _tokenEncryption.Encrypt(newTokens.RefreshToken);
            shop.TokenExpiresAt = DateTime.UtcNow.AddSeconds(newTokens.ExpiresIn);
            await _shopRepository.UpdateAsync(shop);
            
            accessToken = newTokens.AccessToken;
            _logger.LogInformation("Refreshed expired token for shop {ShopId}", shopId);
        }
        
        // Sync listings
        var listings = await _etsyService.GetListingsAsync(accessToken, shop.ExternalId);
        
        int importedCount = 0;
        int updatedCount = 0;
        
        foreach (var listing in listings)
        {
            var existingProduct = await _productRepository.GetByExternalListingIdAsync(listing.ListingId, shopId);
            
            if (existingProduct != null)
            {
                // Idempotent update
                existingProduct.Name = listing.Title;
                existingProduct.Description = listing.Description;
                existingProduct.EtsyPrice = listing.Price;
                existingProduct.ImageUrl = listing.ImageUrl;
                existingProduct.IsActive = listing.IsActive;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                
                await _productRepository.UpdateAsync(existingProduct);
                updatedCount++;
            }
            else
            {
                // Create new product
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    ShopId = shopId,
                    ExternalListingId = listing.ListingId,
                    Name = listing.Title,
                    Description = listing.Description,
                    EtsyPrice = listing.Price,
                    ImageUrl = listing.ImageUrl,
                    IsActive = listing.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await _productRepository.AddAsync(product);
                importedCount++;
            }
        }
        
        // Update last sync time
        shop.LastSyncAt = DateTime.UtcNow;
        await _shopRepository.UpdateAsync(shop);
        
        _logger.LogInformation("Sync completed for shop {ShopId}: {Imported} imported, {Updated} updated", 
            shopId, importedCount, updatedCount);
        
        return new SyncResponse
        {
            JobId = $"sync_{shopId:N}_{DateTime.UtcNow.Ticks}",
            Status = "Completed"
        };
    }
    private static string GenerateCodeVerifier()
    {
        const int length = 128;
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
        var result = new char[length];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var buffer = new byte[length];
        rng.GetBytes(buffer);
        for (int i = 0; i < length; i++)
        {
            result[i] = chars[buffer[i] % chars.Length];
        }
        return new string(result);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
