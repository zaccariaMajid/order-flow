namespace OrderService.Domain.Exceptions;

public sealed class EmptyOrderException : DomainException
{
    public EmptyOrderException()
        : base("An order must contain at least one item.")
    {
    }
}
