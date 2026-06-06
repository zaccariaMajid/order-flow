namespace OrderService.Domain.Orders;

public sealed record OrderPaymentStarted(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : IDomainEvent;
