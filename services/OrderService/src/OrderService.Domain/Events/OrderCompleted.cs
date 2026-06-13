using BuildingBlocks.Domain;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderCompleted(
    OrderId OrderId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
