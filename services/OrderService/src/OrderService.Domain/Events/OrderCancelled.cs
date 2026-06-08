using BuildingBlocks.Domain;

namespace OrderService.Domain.Events;

public sealed record OrderCancelled(
    Guid OrderId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
