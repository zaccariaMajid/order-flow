namespace BuildingBlocks.Infrastructure;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
