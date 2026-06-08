namespace OrderService.Domain.ValueObjects;

public readonly record struct CustomerId
{
    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Customer id cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }
}
