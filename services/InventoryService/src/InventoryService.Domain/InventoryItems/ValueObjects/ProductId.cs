namespace InventoryService.Domain.Inventory;

public sealed record ProductId
{
    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("Product identifier is required.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
