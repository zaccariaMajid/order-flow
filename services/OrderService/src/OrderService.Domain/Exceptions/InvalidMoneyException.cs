namespace OrderService.Domain.Exceptions;

public sealed class InvalidMoneyException : DomainException
{
    public InvalidMoneyException(string message)
        : base(message)
    {
    }
}
