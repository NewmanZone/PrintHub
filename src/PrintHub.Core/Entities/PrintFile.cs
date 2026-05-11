namespace PrintHub.Core.Entities;

public class PrintFile
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string BlobUri { get; set; } = string.Empty;
    public int VersionNumber { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
