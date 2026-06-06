namespace OrderService.Domain.Orders;

public sealed record OrderCompleted(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : IDomainEvent;
