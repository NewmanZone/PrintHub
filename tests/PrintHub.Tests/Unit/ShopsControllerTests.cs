using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PrintHub.API.Controllers;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services.Etsy;
using Xunit;

namespace PrintHub.Tests.Unit;

[Collection("Unit Tests")]
public class ShopsControllerTests
{
    private readonly Mock<IShopService> _shopServiceMock;
    private readonly Mock<ILogger<ShopsController>> _loggerMock;
    private readonly ShopsController _controller;

    public ShopsControllerTests()
    {
        _shopServiceMock = new Mock<IShopService>();
        _loggerMock = new Mock<ILogger<ShopsController>>();
        _controller = new ShopsController(_shopServiceMock.Object, _loggerMock.Object);
        
        // Set up authenticated user
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private Guid GetUserId() => Guid.Parse(_controller.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Fact]
    public async Task GetShops_ReturnsOkWithShops()
    {
        // Arrange
        var userId = GetUserId();
        var shops = new List<ShopResponse>
        {
            new() { Id = Guid.NewGuid(), Provider = "etsy", ShopName = "Test Shop", IsActive = true }
        };
        _shopServiceMock.Setup(s => s.GetShopsAsync(userId)).ReturnsAsync(shops);

        // Act
        var result = await _controller.GetShops();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ShopsResponse>().Subject;
        response.Shops.Should().HaveCount(1);
        response.Shops[0].ShopName.Should().Be("Test Shop");
    }

    [Fact]
    public async Task ConnectEtsy_ReturnsOkWithAuthUrl()
    {
        // Arrange
        var userId = GetUserId();
        var expectedUrl = "https://www.etsy.com/oauth2/authorize?...";
        _shopServiceMock.Setup(s => s.InitiateEtsyConnectAsync(userId, null))
            .ReturnsAsync(new ConnectResponse { AuthUrl = expectedUrl });

        // Act
        var result = await _controller.ConnectEtsy();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ConnectResponseDto>().Subject;
        response.AuthUrl.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task ConnectEtsy_WithReturnUrl_PassesReturnUrl()
    {
        // Arrange
        var userId = GetUserId();
        var returnUrl = "http://localhost:3000/dashboard";
        _shopServiceMock.Setup(s => s.InitiateEtsyConnectAsync(userId, returnUrl))
            .ReturnsAsync(new ConnectResponse { AuthUrl = "https://etsy.com/auth" });

        // Act
        var result = await _controller.ConnectEtsy(new ConnectEtsyRequest { ReturnUrl = returnUrl });

        // Assert
        _shopServiceMock.Verify(s => s.InitiateEtsyConnectAsync(userId, returnUrl), Times.Once);
    }

    [Fact]
    public async Task EtsyCallback_MissingCodeOrState_ReturnsBadRequest()
    {
        // Arrange
        var request = new EtsyCallbackRequest { Code = "", State = "valid_state" };

        // Act
        var result = await _controller.EtsyCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task EtsyCallback_ValidRequest_ReturnsCallbackResponse()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.HandleEtsyCallbackAsync("valid_code", "valid_state"))
            .ReturnsAsync(new CallbackResponse { ShopId = shopId, ShopName = "Test Shop", Connected = true });

        var request = new EtsyCallbackRequest { Code = "valid_code", State = "valid_state" };

        // Act
        var result = await _controller.EtsyCallback(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CallbackResponseDto>().Subject;
        response.ShopId.Should().Be(shopId);
        response.Connected.Should().BeTrue();
    }

    [Fact]
    public async Task EtsyCallback_InvalidState_ReturnsBadRequest()
    {
        // Arrange
        _shopServiceMock.Setup(s => s.HandleEtsyCallbackAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Invalid or expired OAuth state"));

        var request = new EtsyCallbackRequest { Code = "code", State = "invalid_state" };

        // Act
        var result = await _controller.EtsyCallback(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteShop_ExistingShop_ReturnsNoContent()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.DeleteShopAsync(userId, shopId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteShop(shopId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteShop_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.DeleteShopAsync(userId, shopId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.DeleteShop(shopId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteShop_NotOwner_ReturnsForbid()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.DeleteShopAsync(userId, shopId))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.DeleteShop(shopId);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Sync_ExistingShop_ReturnsAccepted()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.InitiateSyncAsync(userId, shopId))
            .ReturnsAsync(new SyncResponse { JobId = "job_123", Status = "Completed" });

        // Act
        var result = await _controller.Sync(shopId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SyncResponseDto>().Subject;
        response.JobId.Should().Be("job_123");
        response.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Sync_NonExistentShop_ReturnsNotFound()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.InitiateSyncAsync(userId, shopId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.Sync(shopId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Sync_TokenExpired_ReturnsBadRequest()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.InitiateSyncAsync(userId, shopId))
            .ThrowsAsync(new EtsyTokenExpiredException("Token expired"));

        // Act
        var result = await _controller.Sync(shopId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Sync_RateLimitExceeded_Returns429()
    {
        // Arrange
        var userId = GetUserId();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.InitiateSyncAsync(userId, shopId))
            .ThrowsAsync(new EtsyRateLimitException("Rate limited"));

        // Act
        var result = await _controller.Sync(shopId);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(429);
    }

    private ShopsController CreateController(IEnumerable<Claim>? claims = null)
    {
        var controller = new ShopsController(_shopServiceMock.Object, _loggerMock.Object);
        var principalClaims = claims ?? Array.Empty<Claim>();
        var identity = new ClaimsIdentity(principalClaims, claims == null ? null : "Test");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        return controller;
    }

    [Fact]
    public async Task EtsyCallback_AllowsAnonymousUser()
    {
        // Arrange
        var anonymousController = CreateController();
        var shopId = Guid.NewGuid();
        _shopServiceMock.Setup(s => s.HandleEtsyCallbackAsync("code", "state"))
            .ReturnsAsync(new CallbackResponse { ShopId = shopId, ShopName = "Test Shop", Connected = true });

        var request = new EtsyCallbackRequest { Code = "code", State = "state" };

        // Act
        var result = await anonymousController.EtsyCallback(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<CallbackResponseDto>().Subject;
        response.Connected.Should().BeTrue();
    }

    [Fact]
    public async Task GetShops_MissingNameIdentifierClaim_ReturnsUnauthorized()
    {
        // Arrange
        var unauthenticatedController = CreateController(new List<Claim>());

        // Act
        var result = await unauthenticatedController.GetShops();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
    }
}
