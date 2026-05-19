namespace PrintHub.Core.Entities;

public class PrintJob
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ShopId { get; set; }
    public string Status { get; set; } = "pending";
    public string? PrinterTarget { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? EstimatedMinutes { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public ICollection<PrintJobItem> Items { get; set; } = new List<PrintJobItem>();
}

public class PrintJobItem
{
    public Guid Id { get; set; }
    public Guid PrintJobId { get; set; }
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Navigation
    public PrintJob? PrintJob { get; set; }
    public Part? Part { get; set; }
}

public class PersonalizedOrder
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string ExternalOrderId { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string Status { get; set; } = "received";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ShippedAt { get; set; }
}

public class PrintFile
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PrintFileVersion
{
    public Guid Id { get; set; }
    public Guid PrintFileId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CostRecord
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class InventoryMovement
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public string Type { get; set; } = string.Empty; // "print" or "manual"
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Shop
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = "etsy";
    public string ExternalId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSyncAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class EtsyOAuthState
{
    public string State { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a reusable component that can be printed.
/// </summary>
public class Part
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsGeneric { get; set; } = false;
    public Guid? CurrentVersionId { get; set; }
    public decimal CostPerUnit { get; set; } = 0;
    public int InventoryOnHand { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Shop? Shop { get; set; }
    public ICollection<ProductPart> ProductParts { get; set; } = new List<ProductPart>();
}

/// <summary>
/// Junction table linking Products to Parts with quantities.
/// </summary>
public class ProductPart
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid PartId { get; set; }
    public int QuantityPerProduct { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
    
    // Navigation
    public Product? Product { get; set; }
    public Part? Part { get; set; }
}

/// <summary>
/// Represents a product imported from Etsy or created locally.
/// </summary>
public class Product
{
    public Guid Id { get; set; }
    public Guid ShopId { get; set; }
    public string? ExternalListingId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? EtsyPrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int PrintCount { get; set; } = 0;
    public int InventoryOnHand { get; set; } = 0;
    public int? ReorderPoint { get; set; }
    public int? ReorderQuantity { get; set; }
    public decimal? CostPerPrint { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public Shop? Shop { get; set; }
    public ICollection<ProductPart> ProductParts { get; set; } = new List<ProductPart>();
}