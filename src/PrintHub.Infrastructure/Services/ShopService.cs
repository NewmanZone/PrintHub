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
        return ToResponses(shops);
    }

    public async Task<IEnumerable<ShopResponse>> GetWorkspaceShopsAsync(Guid workspaceId)
    {
        var shops = await _shopRepository.GetByWorkspaceIdAsync(workspaceId);
        return ToResponses(shops);
    }

    private string GetRedirectUri()
    {
        if (!string.IsNullOrEmpty(_etsyConfig.RedirectUri))
            return _etsyConfig.RedirectUri;

        return _etsyConfig.RedirectFallbackUri;
    }

    public async Task<ConnectResponse> InitiateEtsyConnectAsync(Guid userId, string? returnUrl = null)
        => await InitiateEtsyConnectAsync(userId, Guid.Empty, returnUrl);

    public async Task<ConnectResponse> InitiateEtsyConnectAsync(Guid userId, Guid workspaceId, string? returnUrl = null)
    {
        _logger.LogInformation("Initiating Etsy OAuth flow for user {UserId} in workspace {WorkspaceId}", userId, workspaceId);
        
        // Generate state and PKCE verifier for OAuth security
        var state = Guid.NewGuid().ToString("N");
        var codeVerifier = GenerateCodeVerifier();
        
        // Store state with user context and PKCE verifier
        if (workspaceId == Guid.Empty)
        {
            _oauthStateStore.SaveState(state, userId.ToString(), returnUrl ?? string.Empty, TimeSpan.FromMinutes(10), codeVerifier);
        }
        else
        {
            _oauthStateStore.SaveState(state, userId.ToString(), workspaceId, returnUrl ?? string.Empty, TimeSpan.FromMinutes(10), codeVerifier);
        }
        
        // Build authorization URL with PKCE challenge
        var redirectUri = GetRedirectUri();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var authUrl = await _etsyService.GetAuthorizationUrlAsync(state, redirectUri, codeChallenge);
        
        return new ConnectResponse { AuthUrl = authUrl };
    }

    public async Task<CallbackResponse> HandleEtsyCallbackAsync(string code, string state)
    {
        var (userIdStr, _, codeVerifier) = _oauthStateStore.GetState(state);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogWarning("Invalid or expired OAuth state: {State}", state);
            throw new InvalidOperationException("Invalid or expired OAuth state");
        }

        return await CompleteEtsyCallbackAsync(userId, Guid.Empty, code, state, codeVerifier);
    }

    public async Task<CallbackResponse> HandleEtsyCallbackAsync(Guid userId, Guid workspaceId, string code, string state)
    {
        _logger.LogInformation("Handling Etsy OAuth callback with state {State}", state);
        
        // Validate state and get user context + PKCE verifier
        var (stateUserIdStr, stateWorkspaceId, _, codeVerifier) = _oauthStateStore.GetWorkspaceState(state);
        if (string.IsNullOrEmpty(stateUserIdStr)
            || !Guid.TryParse(stateUserIdStr, out var stateUserId)
            || stateUserId != userId
            || stateWorkspaceId != workspaceId)
        {
            _logger.LogWarning("Invalid or expired OAuth state: {State}", state);
            throw new InvalidOperationException("Invalid or expired OAuth state");
        }

        return await CompleteEtsyCallbackAsync(userId, workspaceId, code, state, codeVerifier);
    }

    private async Task<CallbackResponse> CompleteEtsyCallbackAsync(Guid userId, Guid workspaceId, string code, string state, string? codeVerifier)
    {
        // Clean up state
        _oauthStateStore.DeleteState(state);
        
        // Exchange code for tokens (PKCE)
        var tokenResponse = await _etsyService.ExchangeCodeForTokenAsync(code, GetRedirectUri(), codeVerifier);
        
        // Get shop info from Etsy
        var shopInfo = await _etsyService.GetShopInfoAsync(tokenResponse.AccessToken);
        
        Shop? existingShop;
        if (workspaceId == Guid.Empty)
        {
            existingShop = (await _shopRepository.GetByUserIdAsync(userId)).FirstOrDefault(s => s.ExternalId == shopInfo.ShopId);
        }
        else
        {
            var activeWorkspaceShops = (await _shopRepository.GetByWorkspaceIdAsync(workspaceId))
                .Where(s => s.IsActive)
                .ToList();
            existingShop = activeWorkspaceShops.FirstOrDefault(s => s.ExternalId == shopInfo.ShopId);
            if (existingShop is null && activeWorkspaceShops.Count > 0)
            {
                _logger.LogWarning("Workspace {WorkspaceId} already has an active Etsy shop and cannot connect external shop {ExternalShopId}",
                    workspaceId, shopInfo.ShopId);
                throw new InvalidOperationException("Workspace already has an active Etsy shop connected.");
            }
        }
        
        var shopId = existingShop?.Id ?? Guid.NewGuid();
        var isNewShop = existingShop == null;
        
        // Encrypt tokens before storing
        var encryptedAccessToken = _tokenEncryption.Encrypt(tokenResponse.AccessToken);
        var encryptedRefreshToken = _tokenEncryption.Encrypt(tokenResponse.RefreshToken ?? string.Empty);
        
        var shop = new Shop
        {
            Id = shopId,
            UserId = userId,
            WorkspaceId = workspaceId,
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
            _logger.LogInformation("Created new shop {ShopId} for user {UserId} in workspace {WorkspaceId}", shopId, userId, workspaceId);
        }
        else
        {
            await _shopRepository.UpdateAsync(shop);
            _logger.LogInformation("Updated existing shop {ShopId} for user {UserId} in workspace {WorkspaceId}", shopId, userId, workspaceId);
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
        
        await DeleteShopProductsAsync(shopId);
        await _shopRepository.DeleteAsync(shopId);
    }

    public async Task DeleteWorkspaceShopAsync(Guid workspaceId, Guid shopId)
    {
        _logger.LogInformation("Deleting shop {ShopId} in workspace {WorkspaceId}", shopId, workspaceId);

        var shop = await GetWorkspaceShopOrThrowAsync(workspaceId, shopId);
        await DeleteShopProductsAsync(shop.Id);
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
        
        return await SyncShopAsync(shop);
    }

    public async Task<SyncResponse> InitiateWorkspaceSyncAsync(Guid workspaceId, Guid shopId)
    {
        _logger.LogInformation("Initiating sync for shop {ShopId} in workspace {WorkspaceId}", shopId, workspaceId);
        var shop = await GetWorkspaceShopOrThrowAsync(workspaceId, shopId);
        return await SyncShopAsync(shop);
    }

    private async Task<Shop> GetWorkspaceShopOrThrowAsync(Guid workspaceId, Guid shopId)
    {
        var shop = await _shopRepository.GetByIdAsync(shopId);
        if (shop == null)
        {
            throw new KeyNotFoundException($"Shop {shopId} not found");
        }

        if (shop.WorkspaceId != workspaceId)
        {
            _logger.LogWarning("Attempted workspace-scoped operation for shop {ShopId} in workspace {WorkspaceId}, but shop belongs to {OwnerWorkspaceId}",
                shopId, workspaceId, shop.WorkspaceId);
            throw new UnauthorizedAccessException("You do not have permission to access this shop");
        }

        return shop;
    }

    private async Task DeleteShopProductsAsync(Guid shopId)
    {
        var products = await _productRepository.GetByShopIdWithPartsAsync(shopId);
        foreach (var product in products)
        {
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
    }

    private async Task<SyncResponse> SyncShopAsync(Shop shop)
    {
        var accessToken = _tokenEncryption.Decrypt(shop.AccessToken);

        var isTokenValid = await _etsyService.ValidateTokenAsync(accessToken);
        if (!isTokenValid)
        {
            var refreshToken = _tokenEncryption.Decrypt(shop.RefreshToken);
            var newTokens = await _etsyService.RefreshTokenAsync(refreshToken);

            shop.AccessToken = _tokenEncryption.Encrypt(newTokens.AccessToken);
            shop.RefreshToken = _tokenEncryption.Encrypt(newTokens.RefreshToken ?? string.Empty);
            shop.TokenExpiresAt = DateTime.UtcNow.AddSeconds(newTokens.ExpiresIn);
            await _shopRepository.UpdateAsync(shop);

            accessToken = newTokens.AccessToken;
            _logger.LogInformation("Refreshed expired token for shop {ShopId}", shop.Id);
        }

        var listings = await _etsyService.GetListingsAsync(accessToken, shop.ExternalId);
        var importedCount = 0;
        var updatedCount = 0;

        foreach (var listing in listings)
        {
            var existingProduct = await _productRepository.GetByExternalListingIdAsync(listing.ListingId, shop.Id);

            if (existingProduct != null)
            {
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
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    ShopId = shop.Id,
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

        shop.LastSyncAt = DateTime.UtcNow;
        await _shopRepository.UpdateAsync(shop);

        _logger.LogInformation("Sync completed for shop {ShopId}: {Imported} imported, {Updated} updated",
            shop.Id, importedCount, updatedCount);

        return new SyncResponse
        {
            JobId = $"sync_{shop.Id:N}_{DateTime.UtcNow.Ticks}",
            Status = "Completed"
        };
    }

    private static IEnumerable<ShopResponse> ToResponses(IEnumerable<Shop> shops) =>
        shops.Select(s => new ShopResponse
        {
            Id = s.Id,
            Provider = s.Provider,
            ExternalId = s.ExternalId,
            ShopName = s.ShopName,
            IsActive = s.IsActive,
            LastSyncAt = s.LastSyncAt
        });
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
