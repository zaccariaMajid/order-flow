namespace InventoryService.Domain.Inventory;

public sealed record Sku
{
    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("SKU is required.");
        }

        Value = value.Trim().ToUpperInvariant();
        if (Value.Length > 64)
        {
            throw new DomainException("SKU cannot exceed 64 characters.");
        }
    }

    public string Value { get; }

    public override string ToString() => Value;
}
