namespace InventoryService.Domain.Inventory;

public sealed record OrderId
{
    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("Order identifier is required.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
