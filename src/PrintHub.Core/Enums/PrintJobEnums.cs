namespace PrintHub.Core.Enums;

public enum PrintJobStatus
{
    Pending = 0,
    Queued = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Paused = 6
}

public enum PersonalizedOrderStatus
{
    Received = 0,
    InPreparation = 1,
    QueuedForPrint = 2,
    Printed = 3,
    Shipped = 4
}