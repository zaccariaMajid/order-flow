namespace BuildingBlocks.Domain;

public interface IDomainEvent
{
    Guid Id { get; }

    DateTimeOffset OccurredAt { get; }
}

public abstract class Entity
{
    public Guid Id { get; protected init; } = Guid.NewGuid();
}
