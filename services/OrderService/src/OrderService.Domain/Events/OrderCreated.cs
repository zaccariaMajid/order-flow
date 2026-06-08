using BuildingBlocks.Domain;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderCreated(
    Guid OrderId,
    CustomerId CustomerId,
    IReadOnlyCollection<OrderItem> Items,
    Money TotalAmount,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
