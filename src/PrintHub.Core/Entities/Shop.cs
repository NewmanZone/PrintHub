using PrintHub.Core.Enums;

namespace PrintHub.Core.Entities;

public class Shop
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public ShopProvider Provider { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
