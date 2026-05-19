using FluentAssertions;
using PrintHub.Core.Entities;
using PrintHub.Core.Enums;
using PrintHub.Core.Interfaces;
using PrintHub.Infrastructure.Services;
using PrintHub.Core.Interfaces.Repositories;
using Xunit;

namespace PrintHub.Tests.Queue;

public class BomConsolidationServiceTests
{
    [Fact]
    public void Consolidate_SingleProduct_ReturnsCorrectParts()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var partId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>
        {
            [partId] = new Part
            {
                Id = partId,
                ShopId = shopId,
                Name = "Generic Hook",
                IsGeneric = true,
                CostPerUnit = 0.15m,
                InventoryOnHand = 10,
                CurrentVersionId = Guid.NewGuid()
            }
        };

        var productParts = new Dictionary<Guid, List<ProductPart>>
        {
            [productId] = new List<ProductPart>
            {
                new ProductPart { PartId = partId, QuantityPerProduct = 1 }
            }
        };

        var items = new List<QueueItem>
        {
            new(productId,  5)
        };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert
        results.Should().HaveCount(1);
        var result = results[0];
        result.PartId.Should().Be(partId);
        result.TotalQuantityNeeded.Should().Be(5);
        result.OnHandInventory.Should().Be(10);
        result.NetInventoryAfterPrint.Should().Be(15);
        result.InventoryStatus.Should().Be(PartInventoryStatus.Ready);
    }

    [Fact]
    public void Consolidate_MultipleProductsWithSharedPart_AggregatesCorrectly()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var genericHookId = Guid.NewGuid();
        var dinoCharId = Guid.NewGuid();
        var catCharId = Guid.NewGuid();
        var productDino = Guid.NewGuid();
        var productCat = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>
        {
            [genericHookId] = new Part
            {
                Id = genericHookId,
                ShopId = shopId,
                Name = "Generic Hook",
                IsGeneric = true,
                CostPerUnit = 0.15m,
                InventoryOnHand = 12,
                CurrentVersionId = Guid.NewGuid()
            },
            [dinoCharId] = new Part
            {
                Id = dinoCharId,
                ShopId = shopId,
                Name = "Dino Character",
                IsGeneric = false,
                CostPerUnit = 0.30m,
                InventoryOnHand = 0,
                CurrentVersionId = Guid.NewGuid()
            },
            [catCharId] = new Part
            {
                Id = catCharId,
                ShopId = shopId,
                Name = "Cat Character",
                IsGeneric = false,
                CostPerUnit = 0.25m,
                InventoryOnHand = 2,
                CurrentVersionId = Guid.NewGuid()
            }
        };

        var productParts = new Dictionary<Guid, List<ProductPart>>
        {
            [productDino] = new List<ProductPart>
            {
                new ProductPart { PartId = genericHookId, QuantityPerProduct = 1 },
                new ProductPart { PartId = dinoCharId, QuantityPerProduct = 1 }
            },
            [productCat] = new List<ProductPart>
            {
                new ProductPart { PartId = genericHookId, QuantityPerProduct = 1 },
                new ProductPart { PartId = catCharId, QuantityPerProduct = 1 }
            }
        };

        // User wants: 5x Dino, 3x Cat
        var items = new List<QueueItem>
        {
            new(productDino,  5),
            new(productCat,  3)
        };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert
        results.Should().HaveCount(3);

        // Generic hook should be aggregated: 5 + 3 = 8
        var genericHook = results.First(r => r.PartId == genericHookId);
        genericHook.TotalQuantityNeeded.Should().Be(8);
        genericHook.OnHandInventory.Should().Be(12);
        genericHook.NetInventoryAfterPrint.Should().Be(20);
        genericHook.InventoryStatus.Should().Be(PartInventoryStatus.Ready);
        genericHook.IsGeneric.Should().BeTrue();

        // Dino character: 5 needed, 0 on hand
        var dinoChar = results.First(r => r.PartId == dinoCharId);
        dinoChar.TotalQuantityNeeded.Should().Be(5);
        dinoChar.OnHandInventory.Should().Be(0);
        dinoChar.NetInventoryAfterPrint.Should().Be(5);
        dinoChar.InventoryStatus.Should().Be(PartInventoryStatus.Low);

        // Cat character: 3 needed, 2 on hand
        var catChar = results.First(r => r.PartId == catCharId);
        catChar.TotalQuantityNeeded.Should().Be(3);
        catChar.OnHandInventory.Should().Be(2);
        catChar.NetInventoryAfterPrint.Should().Be(5);
        catChar.InventoryStatus.Should().Be(PartInventoryStatus.Ready);
    }

    [Fact]
    public void Consolidate_EmptyProductParts_ReturnsEmpty()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>();
        var productParts = new Dictionary<Guid, List<ProductPart>>();
        var items = new List<QueueItem> { new(productId,  5) };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void Consolidate_CalculatesEstimatedCost()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var partId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>
        {
            [partId] = new Part
            {
                Id = partId,
                ShopId = shopId,
                Name = "Test Part",
                IsGeneric = false,
                CostPerUnit = 0.50m,
                InventoryOnHand = 0,
                CurrentVersionId = Guid.NewGuid()
            }
        };

        var productParts = new Dictionary<Guid, List<ProductPart>>
        {
            [productId] = new List<ProductPart>
            {
                new ProductPart { PartId = partId, QuantityPerProduct = 1 }
            }
        };

        var items = new List<QueueItem> { new(productId,  10) };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert
        var result = results[0];
        result.TotalQuantityNeeded.Should().Be(10);
        result.EstimatedCost.Should().Be(5.00m);
        result.EstimatedPrintMinutes.Should().Be(50);
    }

    [Fact]
    public void Consolidate_LowInventoryStatus_WhenNetInventoryBelowNeed()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var partId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>
        {
            [partId] = new Part
            {
                Id = partId,
                ShopId = shopId,
                Name = "Test Part",
                IsGeneric = false,
                CostPerUnit = 0.50m,
                InventoryOnHand = 3,
                CurrentVersionId = Guid.NewGuid()
            }
        };

        var productParts = new Dictionary<Guid, List<ProductPart>>
        {
            [productId] = new List<ProductPart>
            {
                new ProductPart { PartId = partId, QuantityPerProduct = 1 }
            }
        };

        // Need 10, have 3 on hand
        var items = new List<QueueItem> { new(productId,  10) };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert
        var result = results[0];
        result.NetInventoryAfterPrint.Should().Be(13); // 3 + 10 = 13
        result.InventoryStatus.Should().Be(PartInventoryStatus.Ready);
    }

    [Fact]
    public void Consolidate_OutOfStockStatus_WhenPartNotInLookup()
    {
        // This tests that missing parts don't cause crashes
        var shopId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var parts = new Dictionary<Guid, Part>(); // Empty - no parts found
        var productParts = new Dictionary<Guid, List<ProductPart>>
        {
            [productId] = new List<ProductPart>
            {
                new ProductPart { PartId = Guid.NewGuid(), QuantityPerProduct = 1 }
            }
        };

        var items = new List<QueueItem> { new(productId,  5) };

        // Act
        var results = BomConsolidationService.Consolidate(shopId, items, productParts, parts);

        // Assert - should be empty since part lookup fails
        results.Should().BeEmpty();
    }
}

public class PrintQueueServiceTests
{
    private readonly MockProductRepository _productRepo;
    private readonly MockPartRepository _partRepo;
    private readonly MockPrintJobRepository _jobRepo;
    private readonly PrintQueueService _service;

    public PrintQueueServiceTests()
    {
        _productRepo = new MockProductRepository();
        _partRepo = new MockPartRepository();
        _jobRepo = new MockPrintJobRepository();
        _service = new PrintQueueService(_productRepo, _partRepo, _jobRepo);
    }

    [Fact]
    public async Task PlanQueueAsync_MultipleProductsWithSharedParts_ConsolidatesBOM()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var genericHookId = Guid.NewGuid();
        var dinoCharId = Guid.NewGuid();

        var genericHook = new Part
        {
            Id = genericHookId,
            ShopId = shopId,
            Name = "Generic Hook",
            IsGeneric = true,
            CostPerUnit = 0.15m,
            InventoryOnHand = 12,
            CurrentVersionId = Guid.NewGuid()
        };

        var dinoChar = new Part
        {
            Id = dinoCharId,
            ShopId = shopId,
            Name = "Dino Character",
            IsGeneric = false,
            CostPerUnit = 0.30m,
            InventoryOnHand = 0,
            CurrentVersionId = Guid.NewGuid()
        };

        await _partRepo.AddAsync(genericHook);
        await _partRepo.AddAsync(dinoChar);

        var productDino = new Product
        {
            Id = Guid.NewGuid(),
            ShopId = shopId,
            Name = "Dino Wall Hook",
            ProductParts = new List<ProductPart>
            {
                new ProductPart { PartId = genericHookId, QuantityPerProduct = 1 },
                new ProductPart { PartId = dinoCharId, QuantityPerProduct = 1 }
            }
        };

        await _productRepo.AddAsync(productDino);

        var items = new List<QueueItem>
        {
            new(productDino.Id,  5)
        };

        // Act
        var result = await _service.PlanQueueAsync(shopId, items);

        // Assert
        result.Should().NotBeNull();
        result.ConsolidatedParts.Should().HaveCount(2);

        var genericPart = result.ConsolidatedParts.First(p => p.PartId == genericHookId);
        genericPart.TotalQuantityNeeded.Should().Be(5);
        genericPart.OnHandInventory.Should().Be(12);
        genericPart.Status.Should().Be(PartInventoryStatus.Ready);

        var dinoPart = result.ConsolidatedParts.First(p => p.PartId == dinoCharId);
        dinoPart.TotalQuantityNeeded.Should().Be(5);
        dinoPart.OnHandInventory.Should().Be(0);
        dinoPart.Status.Should().Be(PartInventoryStatus.Low);
    }

    [Fact]
    public async Task GetQueueStatusAsync_ReturnsCorrectCounts()
    {
        // Arrange
        var shopId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await _jobRepo.AddAsync(new PrintJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            Status = "pending"
        });
        await _jobRepo.AddAsync(new PrintJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            Status = "in_progress"
        });
        await _jobRepo.AddAsync(new PrintJob
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            Status = "completed"
        });

        // Act
        var status = await _service.GetQueueStatusAsync(shopId);

        // Assert
        status.TotalJobs.Should().Be(3);
        status.PendingJobs.Should().Be(1);
        status.ActiveJobs.Should().Be(1);
        status.CompletedJobs.Should().Be(1);
    }

    [Fact]
    public async Task PlanQueueAsync_EmptyItems_ThrowsArgumentException()
    {
        // Arrange
        var shopId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.PlanQueueAsync(shopId, new List<QueueItem>()));
    }
}

// Mock repositories for testing
public class MockProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _products = new();

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<Product?> GetByIdWithPartsAsync(Guid id, CancellationToken ct = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<Product?> GetByExternalListingIdAsync(string externalListingId, Guid shopId, CancellationToken ct = default)
    {
        var product = _products.Values.FirstOrDefault(p => p.ExternalListingId == externalListingId && p.ShopId == shopId);
        return Task.FromResult(product);
    }

    public Task<IEnumerable<Product>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        var products = _products.Values.Where(p => p.ShopId == shopId);
        return Task.FromResult(products);
    }

    public Task<IEnumerable<Product>> GetByShopIdWithPartsAsync(Guid shopId, CancellationToken ct = default)
    {
        return GetByShopIdAsync(shopId, ct);
    }

    public Task<IEnumerable<Product>> SearchByNameAsync(Guid shopId, string searchTerm, CancellationToken ct = default)
    {
        var products = _products.Values.Where(p => p.ShopId == shopId && p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(products);
    }

    public Task<IEnumerable<Product>> GetBelowReorderPointAsync(Guid shopId, CancellationToken ct = default)
    {
        var products = _products.Values.Where(p => p.ShopId == shopId && p.ReorderPoint.HasValue && p.InventoryOnHand < p.ReorderPoint.Value);
        return Task.FromResult(products);
    }

    public Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task<Product> UpdateAsync(Product product, CancellationToken ct = default)
    {
        _products[product.Id] = product;
        return Task.FromResult(product);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _products.Remove(id);
        return Task.CompletedTask;
    }
}

public class MockPartRepository : IPartRepository
{
    private readonly Dictionary<Guid, Part> _parts = new();


    public Task<Part?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _parts.TryGetValue(id, out var part);
        return Task.FromResult(part);
    }

    public Task<Part?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct = default)
    {
        _parts.TryGetValue(id, out var part);
        return Task.FromResult(part);
    }

    public Task<IEnumerable<Part>> GetByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        var parts = _parts.Values.Where(p => p.ShopId == shopId);
        return Task.FromResult(parts);
    }

    public Task<IEnumerable<Part>> GetGenericByShopIdAsync(Guid shopId, CancellationToken ct = default)
    {
        var parts = _parts.Values.Where(p => p.ShopId == shopId && p.IsGeneric);
        return Task.FromResult(parts);
    }

    public Task<IEnumerable<Part>> GetWithLowStockAsync(Guid shopId, CancellationToken ct = default)
    {
        var parts = _parts.Values.Where(p => p.ShopId == shopId && p.InventoryOnHand <= 0);
        return Task.FromResult(parts);
    }

    public Task<Part> AddAsync(Part part, CancellationToken ct = default)
    {
        _parts[part.Id] = part;
        return Task.FromResult(part);
    }

    public Task<Part> UpdateAsync(Part part, CancellationToken ct = default)
    {
        _parts[part.Id] = part;
        return Task.FromResult(part);
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _parts.Remove(id);
        return Task.CompletedTask;
    }
}

public class MockPrintJobRepository : IPrintJobRepository
{
    private readonly Dictionary<Guid, PrintJob> _jobs = new();

    public Task<PrintJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<PrintJob?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<IEnumerable<PrintJob>> GetByShopIdAsync(Guid shopId, PrintJobStatus? status = null, DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
    {
        var jobs = _jobs.Values.Where(j => j.ShopId == shopId);
        if (status.HasValue)
        {
            var statusStr = status.Value.ToString().ToLower();
            jobs = jobs.Where(j => j.Status.ToLower() == statusStr);
        }
        return Task.FromResult(jobs);
    }

    public Task<PrintJob> AddAsync(PrintJob job, CancellationToken ct = default)
    {
        _jobs[job.Id] = job;
        return Task.FromResult(job);
    }

    public Task<PrintJob> UpdateAsync(PrintJob job, CancellationToken ct = default)
    {
        _jobs[job.Id] = job;
        return Task.FromResult(job);
    }
}