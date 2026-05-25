using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PrintHub.Api;

public sealed class EtsyIntegrationService
{
    private readonly IPrintHubStore _store;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly EtsyOptions _options;
    private readonly ILogger<EtsyIntegrationService> _logger;

    public EtsyIntegrationService(IPrintHubStore store, IHttpClientFactory httpClientFactory, IOptions<EtsyOptions> options, ILogger<EtsyIntegrationService> logger)
    {
        _store = store;
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> CreateAuthorizationUrlAsync(string apiBaseUrl, string? returnUrl, CancellationToken ct)
    {
        EnsureConfigured();
        var codeVerifier = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var stateValue = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var state = await _store.ReadAsync(ct);
        state.OAuthStates.RemoveAll(s => s.ExpiresAt <= DateTimeOffset.UtcNow);
        state.OAuthStates.Add(new OAuthStateRecord(stateValue, codeVerifier, returnUrl ?? _options.FrontendReturnUrl ?? "/settings?etsy=connected", DateTimeOffset.UtcNow.AddMinutes(10)));
        await _store.WriteAsync(state, ct);

        var queryParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = GetRedirectUri(apiBaseUrl),
            ["response_type"] = "code",
            ["scope"] = NormalizeScopes(_options.Scopes),
            ["state"] = SignState(stateValue),
            ["code_challenge"] = Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier))),
            ["code_challenge_method"] = "S256"
        };
        return $"{_options.AuthorizeUrl}?{string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";
    }

    public async Task<EtsyOAuthResult> CompleteOAuthAsync(string apiBaseUrl, string code, string signedState, CancellationToken ct)
    {
        EnsureConfigured();
        if (!TryUnsignState(signedState, out var stateValue)) return new EtsyOAuthResult(false, "Invalid OAuth state.", null);

        var state = await _store.ReadAsync(ct);
        var pending = state.OAuthStates.FirstOrDefault(s => s.State == stateValue && s.ExpiresAt > DateTimeOffset.UtcNow);
        if (pending is null) return new EtsyOAuthResult(false, "OAuth state was not found or expired.", null);

        var token = await ExchangeCodeAsync(code, pending.CodeVerifier, GetRedirectUri(apiBaseUrl), ct);
        var shop = await GetSelfShopAsync(token.AccessToken, ct);
        state.EtsyConnection = new EtsyConnectionRecord(shop.ShopId, shop.ShopName, token.AccessToken, token.RefreshToken, DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn - 60, 60)), DateTimeOffset.UtcNow, null);
        state.OAuthStates.RemoveAll(s => s.State == stateValue);
        await _store.WriteAsync(state, ct);
        return new EtsyOAuthResult(true, null, pending.ReturnUrl);
    }

    public async Task<EtsySyncResponse> SyncListingsAsync(CancellationToken ct)
    {
        var state = await _store.ReadAsync(ct);
        var connection = state.EtsyConnection ?? throw new InvalidOperationException("Connect Etsy before syncing listings.");
        connection = await EnsureAccessTokenAsync(state, connection, ct);
        var listings = await FetchListingsAsync(connection, ct);
        var imported = 0;
        var updated = 0;

        foreach (var listing in listings)
        {
            var existingIndex = state.Products.FindIndex(p => p.ExternalListingId == listing.ExternalListingId);
            if (existingIndex < 0)
            {
                state.Products.Add(listing);
                imported++;
            }
            else
            {
                state.Products[existingIndex] = listing with { Id = state.Products[existingIndex].Id };
                updated++;
            }
        }

        state.EtsyConnection = connection with { LastSyncAt = DateTimeOffset.UtcNow };
        await _store.WriteAsync(state, ct);
        return new EtsySyncResponse(imported, updated, state.Products.Count, DateTimeOffset.UtcNow);
    }

    private async Task<EtsyToken> ExchangeCodeAsync(string code, string codeVerifier, string redirectUri, CancellationToken ct)
    {
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = redirectUri,
            ["code"] = code,
            ["code_verifier"] = codeVerifier
        });
        var response = await _httpClientFactory.CreateClient("Etsy").PostAsync(_options.TokenUrl, body, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Etsy token exchange failed: {StatusCode} {Body}", response.StatusCode, json);
            throw new InvalidOperationException("Etsy token exchange failed.");
        }
        return ReadToken(json);
    }

    private async Task<EtsyConnectionRecord> EnsureAccessTokenAsync(PrintHubState state, EtsyConnectionRecord connection, CancellationToken ct)
    {
        if (connection.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(2)) return connection;
        var body = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _options.ClientId,
            ["refresh_token"] = connection.RefreshToken
        });
        var response = await _httpClientFactory.CreateClient("Etsy").PostAsync(_options.TokenUrl, body, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Etsy token refresh failed: {StatusCode} {Body}", response.StatusCode, json);
            throw new InvalidOperationException("Etsy token refresh failed. Reconnect Etsy.");
        }
        var token = ReadToken(json);
        var refreshed = connection with { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken, ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(token.ExpiresIn - 60, 60)) };
        state.EtsyConnection = refreshed;
        return refreshed;
    }

    private async Task<EtsySelfShop> GetSelfShopAsync(string accessToken, CancellationToken ct)
    {
        var response = await CreateAuthorizedClient(accessToken).GetAsync($"{_options.ApiBaseUrl}/users/__SELF__/shops", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to load Etsy shop: {StatusCode} {Body}", response.StatusCode, json);
            throw new InvalidOperationException("Failed to load Etsy shop.");
        }
        using var doc = JsonDocument.Parse(json);
        var shop = doc.RootElement.GetProperty("results").EnumerateArray().FirstOrDefault();
        return new EtsySelfShop(GetJsonString(shop, "shop_id") ?? GetJsonLong(shop, "shop_id").ToString(), GetJsonString(shop, "shop_name") ?? "Etsy Shop");
    }

    private async Task<IReadOnlyList<ProductRecord>> FetchListingsAsync(EtsyConnectionRecord connection, CancellationToken ct)
    {
        var response = await CreateAuthorizedClient(connection.AccessToken).GetAsync($"{_options.ApiBaseUrl}/shops/{connection.ShopId}/listings/active?limit=100&includes=Images", ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to sync Etsy listings: {StatusCode} {Body}", response.StatusCode, json);
            throw new InvalidOperationException("Failed to sync Etsy listings.");
        }
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("results", out var results)) return [];
        return results.EnumerateArray().Select(item =>
        {
            var listingId = GetJsonString(item, "listing_id") ?? GetJsonLong(item, "listing_id").ToString();
            return new ProductRecord(Guid.NewGuid(), listingId, GetJsonString(item, "title") ?? "Untitled listing", GetJsonString(item, "description"), ReadPrice(item), ReadImage(item), string.Equals(GetJsonString(item, "state"), "active", StringComparison.OrdinalIgnoreCase), DateTimeOffset.UtcNow);
        }).ToList();
    }

    private HttpClient CreateAuthorizedClient(string accessToken)
    {
        var client = _httpClientFactory.CreateClient("Etsy");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Remove("x-api-key");
        client.DefaultRequestHeaders.Add("x-api-key", _options.ClientId);
        return client;
    }

    private string GetRedirectUri(string apiBaseUrl) => string.IsNullOrWhiteSpace(_options.RedirectUri) ? $"{apiBaseUrl}/api/etsy/callback" : _options.RedirectUri;
    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId)) throw new InvalidOperationException("Etsy ClientId is not configured.");
    }

    private string SignState(string state)
    {
        var signature = ComputeHmac(state, GetStateSecret());
        return Base64UrlEncode(Encoding.UTF8.GetBytes($"{state}:{signature}"));
    }

    private bool TryUnsignState(string signedState, out string state)
    {
        state = "";
        try
        {
            var decoded = Encoding.UTF8.GetString(Base64UrlDecode(signedState));
            var parts = decoded.Split(':', 2);
            if (parts.Length != 2) return false;
            var expected = ComputeHmac(parts[0], GetStateSecret());
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(parts[1]))) return false;
            state = parts[0];
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetStateSecret() => !string.IsNullOrWhiteSpace(_options.StateSigningSecret)
        ? _options.StateSigningSecret
        : !string.IsNullOrWhiteSpace(_options.ClientSecret)
            ? _options.ClientSecret
            : "dev-only-printhub-state-signing-secret-please-configure";

    private static EtsyToken ReadToken(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return new EtsyToken(doc.RootElement.GetProperty("access_token").GetString() ?? "", doc.RootElement.GetProperty("refresh_token").GetString() ?? "", doc.RootElement.GetProperty("expires_in").GetInt32());
    }

    private static decimal? ReadPrice(JsonElement item)
    {
        if (!item.TryGetProperty("price", out var price)) return null;
        if (price.ValueKind == JsonValueKind.String && decimal.TryParse(price.GetString(), out var stringPrice)) return stringPrice;
        if (price.ValueKind == JsonValueKind.Object && price.TryGetProperty("amount", out var amount) && price.TryGetProperty("divisor", out var divisor) && divisor.GetDecimal() != 0) return amount.GetDecimal() / divisor.GetDecimal();
        return null;
    }

    private static string? ReadImage(JsonElement item)
    {
        if (!item.TryGetProperty("images", out var images) && !item.TryGetProperty("Images", out images)) return null;
        var first = images.ValueKind == JsonValueKind.Array ? images.EnumerateArray().FirstOrDefault() : default;
        return first.ValueKind == JsonValueKind.Object ? GetJsonString(first, "url_170x135") ?? GetJsonString(first, "url_fullxfull") : null;
    }

    private static string? GetJsonString(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;
        return value.ValueKind switch { JsonValueKind.String => value.GetString(), JsonValueKind.Number => value.GetRawText(), _ => null };
    }

    private static long GetJsonLong(JsonElement element, string property) => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetInt64() : 0;
    private static string NormalizeScopes(string scopes) => string.Join(" ", scopes.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct());
    private static string ComputeHmac(string payload, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
    private static string Base64UrlEncode(byte[] input) => Convert.ToBase64String(input).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] Base64UrlDecode(string input) => Convert.FromBase64String(input.PadRight(input.Length + (4 - input.Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/'));

    private sealed record EtsyToken(string AccessToken, string RefreshToken, int ExpiresIn);
    private sealed record EtsySelfShop(string ShopId, string ShopName);
}
