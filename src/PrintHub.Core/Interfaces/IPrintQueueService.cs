namespace PrintHub.Core.Interfaces;

public record QueueItem(Guid ProductId, int Quantity, string? Notes = null);

public record QueuedPart(
    Guid PartId,
    string PartName,
    bool IsGeneric,
    int TotalQuantityNeeded,
    int OnHandInventory,
    int NetAfterPrint,
    PartInventoryStatus Status);

public record QueuePlanResult(
    Guid JobId,
    List<QueuedPart> ConsolidatedParts,
    int TotalEstimatedMinutes,
    decimal TotalEstimatedCost,
    DateTime CreatedAt);

public enum PartInventoryStatus
{
    Ready,
    Low,
    OutOfStock
}

public interface IPrintQueueService
{
    Task<QueuePlanResult> PlanQueueAsync(Guid shopId, List<QueueItem> items);
    Task<QueueStatusSummary> GetQueueStatusAsync(Guid shopId);
}

public record QueueStatusSummary(
    int TotalJobs,
    int PendingJobs,
    int ActiveJobs,
    int CompletedJobs,
    DateTime LastUpdated);