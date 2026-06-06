namespace InventoryService.Domain.Inventory;

public sealed record AdjustmentReason
{
    public AdjustmentReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Stock adjustment reason is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
