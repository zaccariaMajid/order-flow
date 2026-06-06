namespace OrderService.Domain.Orders;

public sealed record OrderCancelled(
    OrderId OrderId,
    CancellationReason Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
