using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Infrastructure.Services;

public class PrintQueueService : IPrintQueueService
{
    private readonly IProductRepository _productRepository;
    private readonly IPartRepository _partRepository;
    private readonly IPrintJobRepository _printJobRepository;

    public PrintQueueService(
        IProductRepository productRepository,
        IPartRepository partRepository,
        IPrintJobRepository printJobRepository)
    {
        _productRepository = productRepository;
        _partRepository = partRepository;
        _printJobRepository = printJobRepository;
    }

    public async Task<QueuePlanResult> PlanQueueAsync(Guid shopId, List<QueueItem> items)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("At least one queue item is required", nameof(items));

        var partsNeeded = new Dictionary<Guid, int>();
        var products = await _productRepository.GetByShopIdAsync(shopId);
        var productLookup = products.ToDictionary(p => p.Id);

        foreach (var item in items)
        {
            if (!productLookup.TryGetValue(item.ProductId, out var product))
                continue;

            var productWithParts = await _productRepository.GetByIdWithPartsAsync(item.ProductId);
            if (productWithParts == null)
                continue;

            foreach (var productPart in productWithParts.ProductParts)
            {
                var quantityNeeded = productPart.QuantityPerProduct * item.Quantity;
                if (partsNeeded.ContainsKey(productPart.PartId))
                    partsNeeded[productPart.PartId] += quantityNeeded;
                else
                    partsNeeded[productPart.PartId] = quantityNeeded;
            }
        }

        var allParts = await _partRepository.GetByShopIdAsync(shopId);
        var partLookup = allParts.ToDictionary(p => p.Id);

        var consolidatedParts = new List<QueuedPart>();
        var totalEstimatedMinutes = 0;
        decimal totalEstimatedCost = 0;

        foreach (var (partId, totalQuantity) in partsNeeded)
        {
            if (!partLookup.TryGetValue(partId, out var part))
                continue;

            var onHand = part.InventoryOnHand;
            var netAfterPrint = onHand + totalQuantity;
            var status = netAfterPrint <= 0 ? PartInventoryStatus.OutOfStock
                : (netAfterPrint < totalQuantity ? PartInventoryStatus.Low
                : PartInventoryStatus.Ready);

            var estimatedMinutes = totalQuantity * 5;
            var estimatedCost = totalQuantity * part.CostPerUnit;

            totalEstimatedMinutes += estimatedMinutes;
            totalEstimatedCost += estimatedCost;

            consolidatedParts.Add(new QueuedPart(
                part.Id,
                part.Name,
                part.IsGeneric,
                totalQuantity,
                onHand,
                netAfterPrint,
                status));
        }

        return new QueuePlanResult(
            Guid.NewGuid(),
            consolidatedParts,
            totalEstimatedMinutes,
            totalEstimatedCost,
            DateTime.UtcNow);
    }

    public async Task<QueueStatusSummary> GetQueueStatusAsync(Guid shopId)
    {
        var jobs = await _printJobRepository.GetByShopIdAsync(shopId);

        var pendingCount = jobs.Count(j => j.Status == "pending");
        var activeCount = jobs.Count(j => j.Status == "in_progress" || j.Status == "InProgress");
        var completedCount = jobs.Count(j => j.Status == "completed");

        return new QueueStatusSummary(
            TotalJobs: jobs.Count(),
            PendingJobs: pendingCount,
            ActiveJobs: activeCount,
            CompletedJobs: completedCount,
            LastUpdated: DateTime.UtcNow);
    }
}