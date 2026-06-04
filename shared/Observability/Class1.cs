namespace Observability;

public sealed record ServiceDiagnostics(string ServiceName, string CorrelationId, string TraceId);
