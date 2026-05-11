using PrintHub.Core.Entities;

namespace PrintHub.Core.Services;

public static class PrintQueueResolutionService
{
    public static Dictionary<Guid, int> ConsolidateParts(IEnumerable<PrintJobItem> items)
    {
        return items
            .GroupBy(i => i.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
    }
}
