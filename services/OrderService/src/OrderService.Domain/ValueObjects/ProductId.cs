namespace OrderService.Domain.ValueObjects;

public readonly record struct ProductId
{
    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Product id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }
}
