using BuildingBlocks.Domain;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Events;

public sealed record OrderFailed(
    OrderId OrderId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
