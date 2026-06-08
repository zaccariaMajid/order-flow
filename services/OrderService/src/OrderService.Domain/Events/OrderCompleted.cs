using BuildingBlocks.Domain;

namespace OrderService.Domain.Events;

public sealed record OrderCompleted(
    Guid OrderId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
