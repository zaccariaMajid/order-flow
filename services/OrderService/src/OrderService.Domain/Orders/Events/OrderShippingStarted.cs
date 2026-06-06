namespace OrderService.Domain.Orders;

public sealed record OrderShippingStarted(
    OrderId OrderId,
    Guid ShipmentId,
    DateTimeOffset OccurredAt) : IDomainEvent;
