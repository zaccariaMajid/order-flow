namespace OrderService.Domain.Orders;

public sealed record Money
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public Money Multiply(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        return new Money(Amount * quantity, Currency);
    }

    public static Money Sum(IEnumerable<Money> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var items = values.ToArray();
        if (items.Length == 0)
        {
            throw new DomainException("At least one monetary value is required.");
        }

        var currency = items[0].Currency;
        if (items.Any(value => value.Currency != currency))
        {
            throw new DomainException("Cannot sum monetary values with different currencies.");
        }

        return new Money(items.Sum(value => value.Amount), currency);
    }
}
