using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces;

namespace PrintHub.Infrastructure.Services;

/// <summary>
/// Service for Bill of Materials (BOM) consolidation for print queue planning.
/// Consolidates shared parts across multiple products to optimize printing.
/// </summary>
public class BomConsolidationService
{
    /// <summary>
    /// Takes a list of product requests with quantities and consolidates them
    /// into a single list of parts with aggregated quantities (BOM consolidation).
    /// </summary>
    /// <param name="shopId">The shop ID for part lookup</param>
    /// <param name="items">Product requests with quantities</param>
    /// <param name="productParts">All product-part relationships to resolve</param>
    /// <param name="parts">All parts to look up current versions and inventory</param>
    /// <returns>List of consolidated parts with totals</returns>
    public static List<BomConsolidationResult> Consolidate(
        Guid shopId,
        List<QueueItem> items,
        Dictionary<Guid, List<ProductPart>> productParts,
        Dictionary<Guid, Part> parts)
    {
        // Dictionary to aggregate parts: PartId -> total quantity needed
        var consolidated = new Dictionary<Guid, ConsolidatedPart>();

        foreach (var item in items)
        {
            if (!productParts.TryGetValue(item.ProductId, out var partsForProduct))
                continue;

            foreach (var productPart in partsForProduct)
            {
                var quantityNeeded = productPart.QuantityPerProduct * item.Quantity;

                if (consolidated.TryGetValue(productPart.PartId, out var existing))
                {
                    existing.TotalQuantity += quantityNeeded;
                    existing.ProductsUsing.Add((item.ProductId, item.Quantity));
                }
                else
                {
                    consolidated[productPart.PartId] = new ConsolidatedPart
                    {
                        PartId = productPart.PartId,
                        TotalQuantity = quantityNeeded,
                        ProductsUsing = new List<(Guid ProductId, int Qty)> { (item.ProductId, item.Quantity) }
                    };
                }
            }
        }

        // Convert to result with inventory status
        var results = new List<BomConsolidationResult>();
        foreach (var (partId, consolidatedPart) in consolidated)
        {
            if (!parts.TryGetValue(partId, out var part))
                continue;

            var netInventory = part.InventoryOnHand + consolidatedPart.TotalQuantity;
            var status = netInventory <= 0 ? PartInventoryStatus.OutOfStock
                : netInventory < consolidatedPart.TotalQuantity ? PartInventoryStatus.Low
                : PartInventoryStatus.Ready;

            results.Add(new BomConsolidationResult
            {
                PartId = partId,
                PartName = part.Name,
                IsGeneric = part.IsGeneric,
                TotalQuantityNeeded = consolidatedPart.TotalQuantity,
                OnHandInventory = part.InventoryOnHand,
                NetInventoryAfterPrint = netInventory,
                InventoryStatus = status,
                PrintFileVersionId = part.CurrentVersionId,
                EstimatedCost = consolidatedPart.TotalQuantity * part.CostPerUnit,
                EstimatedPrintMinutes = consolidatedPart.TotalQuantity * 5 // rough estimate
            });
        }

        return results;
    }
}

public class ConsolidatedPart
{
    public Guid PartId { get; set; }
    public int TotalQuantity { get; set; }
    public List<(Guid ProductId, int Qty)> ProductsUsing { get; set; } = new();
}

public class BomConsolidationResult
{
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public bool IsGeneric { get; set; }
    public int TotalQuantityNeeded { get; set; }
    public int OnHandInventory { get; set; }
    public int NetInventoryAfterPrint { get; set; }
    public PartInventoryStatus InventoryStatus { get; set; }
    public Guid? PrintFileVersionId { get; set; }
    public decimal EstimatedCost { get; set; }
    public int EstimatedPrintMinutes { get; set; }
}