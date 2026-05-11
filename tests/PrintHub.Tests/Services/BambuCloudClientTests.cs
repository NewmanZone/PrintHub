using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services.Bambu;
using Moq;
using Xunit;

namespace PrintHub.Tests.Services;

public class BambuCloudClientTests
{
    [Fact]
    public async Task AuthenticateAsync_ThrowsNotImplemented()
    {
        var httpClientMock = new HttpClient();
        var loggerMock = new Mock<ILogger<BambuCloudClient>>();
        var options = new PrintHub.Core.Configuration.BambuOptions();
        
        var client = new BambuCloudClient(httpClientMock, loggerMock.Object, options);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            client.AuthenticateAsync("test@example.com", "password"));
    }

    [Fact]
    public async Task RefreshTokenAsync_ThrowsNotImplemented()
    {
        var httpClientMock = new HttpClient();
        var loggerMock = new Mock<ILogger<BambuCloudClient>>();
        var options = new PrintHub.Core.Configuration.BambuOptions();
        
        var client = new BambuCloudClient(httpClientMock, loggerMock.Object, options);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            client.RefreshTokenAsync("refresh-token"));
    }

    [Fact]
    public async Task GetDevicesAsync_ThrowsNotImplemented()
    {
        var httpClientMock = new HttpClient();
        var loggerMock = new Mock<ILogger<BambuCloudClient>>();
        var options = new PrintHub.Core.Configuration.BambuOptions();
        
        var client = new BambuCloudClient(httpClientMock, loggerMock.Object, options);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            client.GetDevicesAsync());
    }

    [Fact]
    public async Task GetDeviceAsync_ThrowsNotImplemented()
    {
        var httpClientMock = new HttpClient();
        var loggerMock = new Mock<ILogger<BambuCloudClient>>();
        var options = new PrintHub.Core.Configuration.BambuOptions();
        
        var client = new BambuCloudClient(httpClientMock, loggerMock.Object, options);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            client.GetDeviceAsync("device-id"));
    }
}