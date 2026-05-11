using PrintHub.Core.Enums;

namespace PrintHub.Core.Entities;

public class PrintJobItem
{
    public Guid Id { get; set; }
    public Guid PrintJobId { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; } = 1;
    public PrintJobItemStatus Status { get; set; } = PrintJobItemStatus.Queued;
}
