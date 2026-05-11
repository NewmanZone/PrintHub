using PrintHub.Core.Entities;
using PrintHub.Core.Services;

namespace PrintHub.Tests.Services;

public class InventoryCalculationServiceTests
{
    [Fact]
    public void CalculateAvailableInventory_WithNoMovements_ReturnsCurrentCount()
    {
        var result = InventoryCalculationService.CalculateAvailableInventory(10, []);

        Assert.Equal(10, result);
    }

    [Fact]
    public void CalculateAvailableInventory_WithPositiveAndNegativeMovements_ReturnsCorrectTotal()
    {
        var movements = new List<InventoryMovement>
        {
            new() { Delta = 5 },
            new() { Delta = -3 }
        };

        var result = InventoryCalculationService.CalculateAvailableInventory(10, movements);

        Assert.Equal(12, result);
    }

    [Fact]
    public void IsLowStock_WhenAtOrBelowThreshold_ReturnsTrue()
    {
        Assert.True(InventoryCalculationService.IsLowStock(5, 5));
        Assert.True(InventoryCalculationService.IsLowStock(3, 5));
    }

    [Fact]
    public void IsLowStock_WhenAboveThreshold_ReturnsFalse()
    {
        Assert.False(InventoryCalculationService.IsLowStock(6, 5));
    }
}
