namespace OrderService.Domain.Orders;

public sealed record CustomerId
{
    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("Customer identifier is required.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
