using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PrintHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private static readonly Action<ILogger, string?, Exception?> LogGettingProducts =
        LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(1, nameof(LogGettingProducts)),
            "Getting products for user {UserId}");

    private static readonly Action<ILogger, string, Exception?> LogGettingProduct =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, nameof(LogGettingProduct)),
            "Getting product {ProductId}");

    private static readonly Action<ILogger, string?, Exception?> LogCreatingProduct =
        LoggerMessage.Define<string?>(
            LogLevel.Information,
            new EventId(3, nameof(LogCreatingProduct)),
            "Creating product for user {UserId}");

    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all products for the authenticated user
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetProducts()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        LogGettingProducts(_logger, userId, null);

        // TODO: Fetch from database
        var products = new List<ProductResponse>
        {
            new() { Id = "prod-1", Name = "Sample Product 1", CreatedAt = DateTime.UtcNow },
            new() { Id = "prod-2", Name = "Sample Product 2", CreatedAt = DateTime.UtcNow }
        };

        return Ok(products);
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProduct(string id)
    {
        LogGettingProduct(_logger, id, null);

        // TODO: Fetch from database
        return Ok(new ProductResponse
        {
            Id = id,
            Name = "Sample Product",
            Description = "A sample product for testing",
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Product name is required" });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        LogCreatingProduct(_logger, userId, null);

        var product = new ProductResponse
        {
            Id = "prod-" + Guid.NewGuid().ToString("N"),
            Name = request.Name,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        return Created($"/api/products/{product.Id}", product);
    }
}

public class ProductResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
