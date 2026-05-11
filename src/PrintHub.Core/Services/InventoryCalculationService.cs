using PrintHub.Core.Entities;

namespace PrintHub.Core.Services;

public static class InventoryCalculationService
{
    public static int CalculateAvailableInventory(int currentCount, IEnumerable<InventoryMovement> movements)
    {
        var totalDelta = movements.Sum(m => m.Delta);
        return currentCount + totalDelta;
    }

    public static bool IsLowStock(int available, int threshold)
    {
        return available <= threshold;
    }
}
