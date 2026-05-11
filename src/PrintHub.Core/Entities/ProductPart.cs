namespace PrintHub.Core.Entities;

public class ProductPart
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid PartId { get; set; }
    public int QuantityRequired { get; set; } = 1;
}
