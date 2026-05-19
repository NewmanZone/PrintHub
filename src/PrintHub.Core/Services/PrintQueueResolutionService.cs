using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Core.Services;

public class PrintQueueResolutionService
{
    private readonly IPartRepository _partRepository;
    private readonly IProductPartRepository _productPartRepository;

    public PrintQueueResolutionService(IPartRepository partRepository, IProductPartRepository productPartRepository)
    {
        _partRepository = partRepository;
        _productPartRepository = productPartRepository;
    }

    /// <summary>
    /// Resolves a list of (productId, quantity) items into consolidated part print requirements.
    /// Shared parts are merged so generic hooks only print once for all products that need them.
    /// </summary>
    public async Task<Dictionary<Guid, ConsolidatedPartPrintJob>> ResolveAsync(
        IEnumerable<(Guid ProductId, int Quantity)> items,
        CancellationToken ct = default)
    {
        var result = new Dictionary<Guid, ConsolidatedPartPrintJob>();
        var partUsages = new Dictionary<Guid, int>();
        var partInfo = new Dictionary<Guid, Part>();

        foreach (var (productId, quantity) in items)
        {
            var productParts = await _productPartRepository.GetByProductIdAsync(productId, ct);
            foreach (var pp in productParts)
            {
                if (!partUsages.ContainsKey(pp.PartId))
                {
                    var part = await _partRepository.GetByIdAsync(pp.PartId, ct);
                    if (part != null)
                    {
                        partInfo[pp.PartId] = part;
                        partUsages[pp.PartId] = 0;
                    }
                }
                partUsages[pp.PartId] += pp.QuantityPerProduct * quantity;
            }
        }

        foreach (var (partId, toPrint) in partUsages)
        {
            var part = partInfo[partId];
            var onHand = part.InventoryOnHand;
            var netAfter = onHand + toPrint;

            result[partId] = new ConsolidatedPartPrintJob
            {
                PartId = partId,
                PartName = part.Name,
                ToPrint = toPrint,
                OnHand = onHand,
                NetAfter = netAfter,
                Status = netAfter >= 0 ? PrintJobReadiness.Ready : PrintJobReadiness.Low
            };
        }

        return result;
    }
}

public class ConsolidatedPartPrintJob
{
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public int ToPrint { get; set; }
    public int OnHand { get; set; }
    public int NetAfter { get; set; }
    public PrintJobReadiness Status { get; set; }
}

public enum PrintJobReadiness
{
    Ready,
    Low,
    Critical
}