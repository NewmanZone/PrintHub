namespace PrintHub.Core.Entities;

public class Part
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
