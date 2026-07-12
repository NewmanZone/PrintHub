using FluentAssertions;
using PrintHub.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PrintHub.Core.Interfaces;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Repositories;
using PrintHub.Infrastructure.Services;
using PrintHub.Infrastructure.Services.Etsy;
using PrintHub.Core.Interfaces.Auth;
using PrintHub.Infrastructure.Auth;
using Xunit;

namespace PrintHub.Tests.Unit;

public class DiResolutionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DiResolutionTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void ShopService_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IShopService>();
        service.Should().NotBeNull().And.BeOfType<ShopService>();
    }

    [Fact]
    public void EtsyApiService_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IEtsyService>();
        service.Should().NotBeNull().And.BeOfType<EtsyApiService>();
    }

    [Fact]
    public void ShopRepository_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IShopRepository>();
        service.Should().NotBeNull().And.BeOfType<InMemoryShopRepository>();
    }

    [Fact]
    public void ProductRepository_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IProductRepository>();
        service.Should().NotBeNull().And.BeOfType<InMemoryProductRepository>();
    }

    [Fact]
    public void TokenEncryptionService_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<ITokenEncryptionService>();
        service.Should().NotBeNull().And.BeOfType<AesTokenEncryptionService>();
    }

    [Fact]
    public void OAuthStateStore_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetService<IOAuthStateStore>();
        service.Should().NotBeNull().And.BeOfType<InMemoryOAuthStateStore>();
    }

    [Fact]
    public void EtsyConfiguration_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetService<EtsyConfiguration>();
        config.Should().NotBeNull();
        config!.BaseUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuthenticationAndWorkspaceServices_CanBeResolved()
    {
        using var scope = _factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IUserRepository>().Should().BeOfType<InMemoryUserRepository>();
        scope.ServiceProvider.GetRequiredService<IWorkspaceRepository>().Should().BeOfType<InMemoryWorkspaceRepository>();
        scope.ServiceProvider.GetRequiredService<ICurrentUserService>().Should().BeOfType<CurrentUserService>();
        scope.ServiceProvider.GetRequiredService<ICurrentUserContext>().Should().BeOfType<HttpCurrentUserContext>();
        scope.ServiceProvider.GetRequiredService<IWorkspaceAuthorizationService>().Should().BeOfType<WorkspaceAuthorizationService>();
    }
}
