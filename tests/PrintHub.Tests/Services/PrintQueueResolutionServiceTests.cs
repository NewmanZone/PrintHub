using PrintHub.Core.Entities;
using PrintHub.Core.Services;

namespace PrintHub.Tests.Services;

public class PrintQueueResolutionServiceTests
{
    [Fact]
    public void ConsolidateParts_GroupsByPartIdAndSumsQuantity()
    {
        var partId = Guid.NewGuid();
        var items = new List<PrintJobItem>
        {
            new() { PartId = partId, Quantity = 2 },
            new() { PartId = partId, Quantity = 3 }
        };

        var result = PrintQueueResolutionService.ConsolidateParts(items);

        Assert.Single(result);
        Assert.Equal(5, result[partId]);
    }

    [Fact]
    public void ConsolidateParts_HandlesMultiplePartIds()
    {
        var partA = Guid.NewGuid();
        var partB = Guid.NewGuid();
        var items = new List<PrintJobItem>
        {
            new() { PartId = partA, Quantity = 1 },
            new() { PartId = partB, Quantity = 2 }
        };

        var result = PrintQueueResolutionService.ConsolidateParts(items);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[partA]);
        Assert.Equal(2, result[partB]);
    }
}
