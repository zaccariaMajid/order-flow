namespace OrderService.Domain.Orders;

public sealed record OrderPaid(
    OrderId OrderId,
    Guid PaymentId,
    DateTimeOffset OccurredAt) : IDomainEvent;
