using PrintHub.Core.Interfaces;
using PrintHub.Core.Interfaces.Repositories;
using PrintHub.Infrastructure.Repositories;
using PrintHub.Infrastructure.Services.Etsy;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Repositories;

namespace PrintHub.Api;

public static class Phase1Api
{
    public static IServiceCollection AddPrintHubPhase1(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:3000", "http://127.0.0.1:3000", "http://127.0.0.1:4175"];
                policy.WithOrigins(origins.Select(origin => origin.TrimEnd('/')).ToArray()).AllowAnyHeader().AllowAnyMethod();
            });
        });
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
            BaseUrl = configuration["Etsy:ApiBaseUrl"] ?? configuration["Etsy:BaseUrl"] ?? "https://openapi.etsy.com/v3/application",
            AuthorizationUrl = configuration["Etsy:AuthorizeUrl"] ?? "https://www.etsy.com/oauth2/authorize",
            TokenUrl = configuration["Etsy:TokenUrl"] ?? "https://api.etsy.com/v3/public/oauth/token",
            Scopes = configuration["Etsy:Scopes"] ?? "listings_r shops_r",
            RedirectUri = configuration["Etsy:RedirectUri"] ?? string.Empty,
        });
        services.AddHttpClient<IEtsyService, EtsyApiService>();
        return services;
    }

    public static IEndpointRouteBuilder MapPrintHubPhase1Api(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "printhub-api" }));
        app.MapGet("/", () => Results.Ok(new { service = "PrintHub API", version = "1.0.0" }));
        app.MapGet("/api/me", () => Results.Ok(PrintHubDefaults.User));
        app.MapGet("/api/etsy/connection", async (IPrintHubStore store, CancellationToken ct) =>
        {
            var state = await store.ReadAsync(ct);
            return Results.Ok(state.EtsyConnection?.ToResponse());
        });
        app.MapGet("/api/etsy/connect", async (HttpRequest request, EtsyIntegrationService etsy, string? returnUrl, CancellationToken ct) =>
        {
            var authUrl = await etsy.CreateAuthorizationUrlAsync(GetApiBaseUrl(request), returnUrl, ct);
            return Results.Ok(new { authUrl });
        });
        app.MapGet("/api/etsy/callback", async (HttpRequest request, EtsyIntegrationService etsy, string code, string state, CancellationToken ct) =>
        {
            var result = await etsy.CompleteOAuthAsync(GetApiBaseUrl(request), code, state, ct);
            return result.Success
                ? Results.Redirect(result.ReturnUrl ?? "/settings?etsy=connected")
                : Results.BadRequest(result);
        });
        app.MapPost("/api/etsy/sync", async (EtsyIntegrationService etsy, CancellationToken ct) =>
        {
            var result = await etsy.SyncListingsAsync(ct);
            return Results.Ok(result);
        });
        app.MapGet("/api/products", async (IProductRepository products, CancellationToken ct) =>
        {
            var importedProducts = await products.GetAllAsync(ct);
            return Results.Ok(new ProductsResponse(importedProducts.OrderBy(p => p.Name).Select(p => p.ToRecord())));
        });
        app.MapGet("/api/products/{productId:guid}", async (Guid productId, IProductRepository products, CancellationToken ct) =>
        {
            var product = await products.GetByIdAsync(productId, ct);
            return product is null ? Results.NotFound(new { error = "Product not found" }) : Results.Ok(product.ToRecord());
        });
        app.MapPost("/api/products/{productId:guid}/files", UploadProductFileAsync);
        app.MapGet("/api/products/{productId:guid}/files", async (Guid productId, IPrintHubStore store, CancellationToken ct) =>
        {
            var state = await store.ReadAsync(ct);
            return Results.Ok(new ProductFilesResponse(state.Files.Where(f => f.ProductId == productId).OrderByDescending(f => f.UploadedAt).Select(f => f.ToResponse())));
        });
        app.MapGet("/api/files/{fileId:guid}/download", async (Guid fileId, IPrintHubStore store, IPrintHubFileStorage fileStorage, CancellationToken ct) =>
        {
            var state = await store.ReadAsync(ct);
            var file = state.Files.FirstOrDefault(f => f.Id == fileId);
            if (file is null) return Results.NotFound(new { error = "File not found" });
            var stream = await fileStorage.OpenReadAsync(file.StoragePath, ct);
            return Results.File(stream, "application/octet-stream", file.FileName);
        });
        return app;
    }

    private static async Task<IResult> UploadProductFileAsync(Guid productId, HttpRequest request, IPrintHubStore store, IProductRepository products, IPrintHubFileStorage fileStorage, CancellationToken ct)
    {
        if (!request.HasFormContentType) return Results.BadRequest(new { error = "Expected multipart form upload." });
        var form = await request.ReadFormAsync(ct);
        var file = form.Files.GetFile("file");
        if (file is null || file.Length == 0) return Results.BadRequest(new { error = "A non-empty file field named 'file' is required." });
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not ".3mf" and not ".stl") return Results.BadRequest(new { error = "Only .3mf and .stl files are supported." });

        if (await products.GetByIdAsync(productId, ct) is null) return Results.NotFound(new { error = "Product not found" });
        var state = await store.ReadAsync(ct);
        await using var stream = file.OpenReadStream();
        var storedPath = await fileStorage.SaveAsync(productId, file.FileName, stream, ct);
        var productFile = new ProductFileRecord(Guid.NewGuid(), productId, file.FileName, extension, file.Length, state.Files.Count(f => f.ProductId == productId) + 1, storedPath, DateTimeOffset.UtcNow);
        state.Files.Add(productFile);
        await store.WriteAsync(state, ct);
        return Results.Created($"/api/products/{productId}/files/{productFile.Id}", productFile.ToResponse());
    }

    private static string GetApiBaseUrl(HttpRequest request)
    {
        var configured = request.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["PublicApiBaseUrl"];
        return !string.IsNullOrWhiteSpace(configured) ? configured.TrimEnd('/') : $"{request.Scheme}://{request.Host}".TrimEnd('/');
    }
}

public sealed record CurrentUserResponse(Guid UserId, Guid WorkspaceId, string DisplayName, string Email);

public static class PrintHubDefaults
{
    public static readonly CurrentUserResponse User = new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        "NewmanZone",
        "mln330@users.noreply.github.com");
}

public sealed record EtsyOptions
{
    public string ClientId { get; init; } = "";
    public string ClientSecret { get; init; } = "";
    public string Scopes { get; init; } = "listings_r shops_r transactions_r";
    public string AuthorizeUrl { get; init; } = "https://www.etsy.com/oauth/connect";
    public string TokenUrl { get; init; } = "https://api.etsy.com/v3/public/oauth/token";
    public string ApiBaseUrl { get; init; } = "https://api.etsy.com/v3/application";
    public string? RedirectUri { get; init; }
    public string? FrontendReturnUrl { get; init; }
    public string StateSigningSecret { get; init; } = "";
}

public sealed record StorageOptions
{
    public string? ConnectionString { get; init; }
    public string ContainerName { get; init; } = "printhub";
    public string LocalPath { get; init; } = "App_Data";
}

public sealed record PrintHubState
{
    public EtsyConnectionRecord? EtsyConnection { get; set; }
    public List<OAuthStateRecord> OAuthStates { get; set; } = [];
    public List<ProductRecord> Products { get; set; } = [];
    public List<ProductFileRecord> Files { get; set; } = [];
}

public sealed record OAuthStateRecord(string State, string CodeVerifier, string ReturnUrl, DateTimeOffset ExpiresAt);
public sealed record EtsyConnectionRecord(string ShopId, string ShopName, string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt, DateTimeOffset ConnectedAt, DateTimeOffset? LastSyncAt);
public sealed record ProductRecord(Guid Id, string ExternalListingId, string Name, string? Description, decimal? EtsyPrice, string? ImageUrl, bool IsActive, DateTimeOffset UpdatedAt);
public sealed record ProductFileRecord(Guid Id, Guid ProductId, string FileName, string FileType, long FileSizeBytes, int VersionNumber, string StoragePath, DateTimeOffset UploadedAt);
public sealed record EtsyConnectionResponse(string ShopId, string ShopName, DateTimeOffset ExpiresAt, DateTimeOffset ConnectedAt, DateTimeOffset? LastSyncAt);
public sealed record ProductFileResponse(Guid Id, Guid ProductId, string FileName, string FileType, long FileSizeBytes, int VersionNumber, DateTimeOffset UploadedAt);
public sealed record ProductsResponse(IEnumerable<ProductRecord> Products);
public sealed record ProductFilesResponse(IEnumerable<ProductFileResponse> Files);
public sealed record EtsySyncResponse(int Imported, int Updated, int Total, DateTimeOffset SyncedAt);
public sealed record EtsyOAuthResult(bool Success, string? Error, string? ReturnUrl);

public static class ApiResponseMapping
{
    public static EtsyConnectionResponse ToResponse(this EtsyConnectionRecord connection) =>
        new(connection.ShopId, connection.ShopName, connection.ExpiresAt, connection.ConnectedAt, connection.LastSyncAt);

    public static ProductRecord ToRecord(this PrintHub.Core.Entities.Product product) =>
        new(product.Id, product.ExternalListingId ?? string.Empty, product.Name, product.Description, product.EtsyPrice, product.ImageUrl, product.IsActive, product.UpdatedAt);

    public static ProductFileResponse ToResponse(this ProductFileRecord file) =>
        new(file.Id, file.ProductId, file.FileName, file.FileType, file.FileSizeBytes, file.VersionNumber, file.UploadedAt);
}

public static class PrintHubAuthentication
{
    public static IServiceCollection AddPrintHubAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authority = configuration["Authentication:Authority"];
        var audience = configuration["Authentication:Audience"];
        if (!string.IsNullOrWhiteSpace(authority) && !string.IsNullOrWhiteSpace(audience))
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                });
            return services;
        }

        services.AddAuthentication(PrintHubAuthDefaults.Scheme)
            .AddScheme<AuthenticationSchemeOptions, PrintHubHeaderAuthenticationHandler>(PrintHubAuthDefaults.Scheme, _ => { });
        return services;
    }
}

public static class PrintHubAuthDefaults
{
    public const string Scheme = "PrintHubHeader";
    public const string UserIdHeader = "X-User-Id";
}

public sealed class PrintHubHeaderAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration,
    IWebHostEnvironment environment) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerAuthEnabled = environment.IsDevelopment()
            || string.Equals(configuration["Auth:AllowHeaderUserId"], "true", StringComparison.OrdinalIgnoreCase);
        if (!headerAuthEnabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.TryGetValue(PrintHubAuthDefaults.UserIdHeader, out var values)
            || !Guid.TryParse(values.FirstOrDefault(), out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
