using PrintHub.Core.Entities;
using PrintHub.Core.Interfaces.Repositories;

namespace PrintHub.Core.Services;

public class InventoryCalculationService
{
    private readonly IPartRepository _partRepository;
    private readonly IProductPartRepository _productPartRepository;

    public InventoryCalculationService(IPartRepository partRepository, IProductPartRepository productPartRepository)
    {
        _partRepository = partRepository;
        _productPartRepository = productPartRepository;
    }

    /// <summary>
    /// Calculates the total cost per print for a product based on its parts' costs and quantities.
    /// </summary>
    public async Task<decimal> CalculateCostPerPrintAsync(Guid productId, CancellationToken ct = default)
    {
        var productParts = await _productPartRepository.GetByProductIdAsync(productId, ct);
        decimal totalCost = 0;

        foreach (var pp in productParts)
        {
            var part = await _partRepository.GetByIdAsync(pp.PartId, ct);
            if (part != null)
            {
                totalCost += part.CostPerUnit * pp.QuantityPerProduct;
            }
        }

        return totalCost;
    }

    /// <summary>
    /// Calculates the total inventory value for a shop (sum of all parts on hand * cost per unit).
    /// </summary>
    public async Task<decimal> CalculateShopInventoryValueAsync(Guid shopId, CancellationToken ct = default)
    {
        var parts = await _partRepository.GetByShopIdAsync(shopId, ct);
        decimal totalValue = 0;

        foreach (var part in parts)
        {
            totalValue += part.CostPerUnit * part.InventoryOnHand;
        }

        return totalValue;
    }
}