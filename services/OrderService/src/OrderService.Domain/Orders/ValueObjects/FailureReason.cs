namespace OrderService.Domain.Orders;

public sealed record FailureReason
{
    public FailureReason(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Failure reason is required.");
        }

        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
