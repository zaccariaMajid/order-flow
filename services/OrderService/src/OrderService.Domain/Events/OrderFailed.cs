using BuildingBlocks.Domain;

namespace OrderService.Domain.Events;

public sealed record OrderFailed(
    Guid OrderId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
