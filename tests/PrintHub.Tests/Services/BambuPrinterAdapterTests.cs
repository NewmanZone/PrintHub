using PrintHub.Core.Interfaces.Adapters;
using PrintHub.Core.Interfaces.Services;
using PrintHub.Infrastructure.Services.Bambu;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace PrintHub.Tests.Services;

public class BambuPrinterAdapterTests
{
    [Fact]
    public void PrinterType_ReturnsBambu()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        Assert.Equal("bambu", adapter.PrinterType);
    }

    [Fact]
    public async Task DiscoverPrintersAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.DiscoverPrintersAsync());
    }

    [Fact]
    public async Task GetStatusAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.GetStatusAsync("test-printer-id"));
    }

    [Fact]
    public async Task StartPrintAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);
        var job = new PrintJob("job-1", "test.3mf", Array.Empty<byte>());

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.StartPrintAsync("test-printer-id", job));
    }

    [Fact]
    public async Task StopPrintAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.StopPrintAsync("test-printer-id"));
    }

    [Fact]
    public async Task PausePrintAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.PausePrintAsync("test-printer-id"));
    }

    [Fact]
    public async Task ResumePrintAsync_ThrowsNotImplemented()
    {
        var loggerMock = new Mock<ILogger<BambuPrinterAdapter>>();
        var adapter = new BambuPrinterAdapter(loggerMock.Object);

        await Assert.ThrowsAsync<NotImplementedException>(() => 
            adapter.ResumePrintAsync("test-printer-id"));
    }
}