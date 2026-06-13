namespace OrderService.Domain.ValueObjects;

public readonly record struct OrderId
{
    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Order id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public static OrderId New() => new(Guid.NewGuid());
}
