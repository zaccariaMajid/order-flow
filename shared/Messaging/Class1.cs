namespace Messaging;

public sealed record IntegrationEvent(
    Guid Id,
    string Type,
    DateTimeOffset OccurredAt,
    string Payload);
