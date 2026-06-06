namespace OrderService.Domain.Orders;

public sealed record OrderStockReserved(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : IDomainEvent;
