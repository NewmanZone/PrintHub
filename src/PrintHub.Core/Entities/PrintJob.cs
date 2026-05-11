using PrintHub.Core.Enums;

namespace PrintHub.Core.Entities;

public class PrintJob
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public PrintJobStatus Status { get; set; } = PrintJobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<PrintJobItem> Items { get; set; } = [];
}
