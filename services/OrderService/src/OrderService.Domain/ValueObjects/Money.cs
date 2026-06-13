using OrderService.Domain.Enums;
using OrderService.Domain.Exceptions;

namespace OrderService.Domain.ValueObjects;

public sealed record Money
{
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
        {
            throw new InvalidMoneyException("Amount cannot be negative.");
        }

        if (!Enum.IsDefined(currency))
        {
            throw new InvalidMoneyException("Currency must be specified.");
        }

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public Currency Currency { get; }

    public static Money Zero(Currency currency) => new(0, currency);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        EnsureSameCurrency(other);

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Multiply(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        return new Money(Amount * quantity, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidMoneyException("Cannot operate on money values with different currencies.");
        }
    }
}
