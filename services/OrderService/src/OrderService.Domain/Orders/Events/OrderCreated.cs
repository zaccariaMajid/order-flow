namespace OrderService.Domain.Orders;

public sealed record OrderCreated(
    OrderId OrderId,
    CustomerId CustomerId,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent;
