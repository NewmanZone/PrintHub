using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Repositories;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Services.Etsy;

namespace PrintHub.Api;

public static class Phase1Api
{
    public static IServiceCollection AddPrintHubPhase1(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 250 * 1024 * 1024);
        services.Configure<EtsyOptions>(configuration.GetSection("Etsy"));
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));
        services.AddPrintHubAuthentication(configuration);
        services.AddAuthorization();
        services.AddHttpClient("Etsy", client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddSingleton<IPrintHubStore, PrintHubStore>();
        services.AddSingleton<IPrintHubFileStorage, PrintHubFileStorage>();
        services.AddSingleton<EtsyIntegrationService>();
        services.AddSingleton<IOAuthStateStore, InMemoryOAuthStateStore>();
        services.AddScoped<IShopService, ShopService>();
        services.AddSingleton<IShopRepository, InMemoryShopRepository>();
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        services.AddSingleton<ITokenEncryptionService>(sp => 
            new AesTokenEncryptionService(configuration["TokenEncryption:Key"] 
                ?? Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))));
        services.AddSingleton<EtsyConfiguration>(sp => new EtsyConfiguration
        {
            ClientId = configuration["Etsy:ClientId"] ?? string.Empty,
            ClientSecret = configuration["Etsy:ClientSecret"] ?? string.Empty,
            BaseUrl = configuration["Etsy:BaseUrl"] ?? configuration["Etsy:ApiBaseUrl"] ?? "https://openapi.etsy.com/v3/application",
            AuthorizationUrl = configuration["Etsy:AuthorizationUrl"] ?? "https://www.etsy.com/oauth2/authorize",
            TokenUrl = configuration["Etsy:TokenUrl"] ?? "https://api.etsy.com/v3/public/oauth/token",
            RedirectUri = configuration["Etsy:RedirectUri"] ?? string.Empty,
            Scopes = configuration["Etsy:Scopes"] ?? "listings_r shops_r",
            UseFakeProvider = bool.TryParse(configuration["Etsy:UseFakeProvider"], out var fake) && fake
        });
        services.AddSingleton<IEtsyService>(sp =>
        {
            var config = sp.GetRequiredService<EtsyConfiguration>();
            if (config.UseFakeProvider)
                return new FakeEtsyService();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("Etsy");
            var logger = sp.GetRequiredService<ILogger<EtsyApiService>>();
            return new EtsyApiService(httpClient, config, logger);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapPrintHubPhase1Api(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");
        app.MapGet("/", () => "PrintHub API");

        // Phase 1: OAuth + Etsy integration
        app.MapGet("/api/me", async (IIdentityService identity, CancellationToken ct) =>
        {
            var user = await identity.GetCurrentUserAsync(ct);
            return user is null ? Results.Unauthorized() : Results.Ok(user);
        }).RequireAuthorization();

        app.MapGet("/api/etsy/connection", [Authorize] async (IShopService shops, CancellationToken ct) =>
        {
            var shopsList = await shops.GetShopsAsync(Guid.Empty, ct);
            return Results.Ok(new { connected = shopsList.Any(), shops = shopsList });
        });

        app.MapPost("/api/etsy/connect", [Authorize] async (IShopService shops, HttpContext ctx, CancellationToken ct) =>
        {
            var userId = GetUserId(ctx.User);
            if (userId == Guid.Empty) return Results.Unauthorized();
            var returnUrl = ctx.Request.Query["returnUrl"].FirstOrDefault() ?? "/";
            var result = await shops.InitiateEtsyConnectAsync(userId, returnUrl, ct);
            return Results.Ok(new { authUrl = result.AuthUrl });
        });

        app.MapGet("/api/etsy/callback", async (IShopService shops, HttpContext ctx, CancellationToken ct) =>
        {
            var code = ctx.Request.Query["code"].FirstOrDefault() ?? "";
            var state = ctx.Request.Query["state"].FirstOrDefault() ?? "";
            var result = await shops.HandleEtsyCallbackAsync(code, state, ct);
            return result.Connected ? Results.Ok(result) : Results.BadRequest(result);
        });

        app.MapPost("/api/etsy/sync", [Authorize] async (EtsyIntegrationService etsy, CancellationToken ct) =>
        {
            var result = await etsy.SyncListingsAsync(ct);
            return Results.Ok(result);
        });

        app.MapGet("/api/products", [Authorize] async (
            IProductRepository products,
            IShopRepository shops,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var userId = GetUserId(ctx.User);
            if (userId == Guid.Empty) return Results.Unauthorized();
            var userShops = await shops.GetByUserIdAsync(userId, ct);
            var shopIds = userShops.Select(s => s.Id).ToHashSet();
            var importedProducts = await products.GetAllAsync(ct);
            var filtered = importedProducts
                .Where(p => shopIds.Contains(p.ShopId))
                .OrderBy(p => p.Name)
                .Select(p => p.ToRecord());
            return Results.Ok(new ProductsResponse(filtered));
        });

        app.MapGet("/api/products/{productId:guid}", [Authorize] async (
            IProductRepository products,
            IShopRepository shops,
            Guid productId,
            HttpContext ctx,
            CancellationToken ct) =>
        {
            var userId = GetUserId(ctx.User);
            if (userId == Guid.Empty) return Results.Unauthorized();
            var userShops = await shops.GetByUserIdAsync(userId, ct);
            var shopIds = userShops.Select(s => s.Id).ToHashSet();
            var product = await products.GetByIdAsync(productId, ct);
            if (product == null) return Results.NotFound();
            if (!shopIds.Contains(product.ShopId)) return Results.Forbid();
            return Results.Ok(product.ToRecord());
        });

        app.MapPost("/api/products/{productId:guid}/files", [Authorize] async (Guid productId, IFormFile file, IPrintHubFileStorage storage, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await storage.UploadAsync(productId, file.FileName, stream, ct);
            return Results.Ok(new { fileUrl = result });
        });

        return app;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var nameId = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return nameId is not null && Guid.TryParse(nameId, out var id) ? id : Guid.Empty;
    }

    // --- DTOs ---
    public record ProductsResponse(IEnumerable<ProductRecord> Products);
    public record ProductRecord(Guid Id, string Name, string? Description, string? ImageUrl, decimal EtsyPrice, bool IsActive);
}

public static class ProductExtensions
{
    public static Phase1Api.ProductRecord ToRecord(this Product p) =>
        new(p.Id, p.Name, p.Description, p.ImageUrl, p.EtsyPrice, p.IsActive);
}
