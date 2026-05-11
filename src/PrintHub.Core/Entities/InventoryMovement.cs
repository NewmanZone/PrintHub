namespace PrintHub.Core.Entities;

public class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
