namespace OrderService.Domain.Orders;

public sealed record OrderFailed(
    OrderId OrderId,
    FailureReason Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
