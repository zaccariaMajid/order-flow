namespace OrderService.Domain.Orders;

public sealed record CancellationReason
{
    public CancellationReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Cancellation reason is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
