# Logging Strategy

## Purpose

Define the logging strategy for the Order Flow platform.

The objective is to provide sufficient observability for:

```text
Application startup
HTTP requests
Database connectivity
Health checks
Unexpected failures
Deployments
```

while keeping the implementation lightweight and cloud-native.

Logs are emitted to stdout and collected by:

```text
Amazon ECS
↓
CloudWatch Logs
```

No external logging provider is required at this stage.

---

# Logging Principles

## Structured Logging

All logs must use structured logging.

Allowed:

```csharp
logger.LogInformation(
    "Order {OrderId} created",
    orderId);
```

Forbidden:

```csharp
logger.LogInformation(
    $"Order {orderId} created");
```

---

## Sensitive Data

Never log:

```text
Connection Strings
Passwords
Tokens
Secrets
API Keys
Personal Data
```

Allowed:

```text
Database configured: true
```

Forbidden:

```text
Host=...
Password=...
```

---

# Logging Categories

## Startup

Startup logs provide visibility into application bootstrapping.

Required:

```text
Application starting

Environment

Application started
```

Example:

```csharp
logger.LogInformation(
    "Order Service starting");

logger.LogInformation(
    "Environment: {Environment}",
    app.Environment.EnvironmentName);
```

---

## Database Configuration

The application must verify that a database connection string is configured.

Required:

```text
Database configured
```

Example:

```csharp
logger.LogInformation(
    "Database configured: {Configured}",
    !string.IsNullOrWhiteSpace(connectionString));
```

Do not log the actual connection string.

---

## Health Checks

Health endpoints must generate logs.

Endpoints:

```http
GET /health

GET /health/ready
```

Required:

```text
Health check executed

Readiness check executed

Database connectivity result
```

Example:

```csharp
logger.LogInformation(
    "Database connectivity check: {CanConnect}",
    canConnect);
```

---

## Database Failures

All database failures must be logged.

Example:

```csharp
catch (Exception ex)
{
    logger.LogError(
        ex,
        "Database readiness check failed");
}
```

Required information:

```text
Exception Type

Message

Stack Trace
```

---

## HTTP Requests

The application must log incoming requests.

Example:

```csharp
app.Use(async (context, next) =>
{
    logger.LogInformation(
        "{Method} {Path}",
        context.Request.Method,
        context.Request.Path);

    await next();
});
```

Expected output:

```text
GET /health

GET /health/ready

POST /orders
```

---

# Log Levels

## Information

Used for:

```text
Startup

Shutdown

Health Checks

Business Events

Request Tracking
```

Example:

```csharp
logger.LogInformation(...)
```

---

## Warning

Used for:

```text
Recoverable Failures

Validation Problems

Transient Issues
```

Example:

```csharp
logger.LogWarning(...)
```

---

## Error

Used for:

```text
Unhandled Exceptions

Database Failures

Infrastructure Failures
```

Example:

```csharp
logger.LogError(...)
```

---

# CloudWatch Integration

Logs are written to:

```text
stdout
```

and automatically collected by:

```text
Amazon ECS
↓
CloudWatch Logs
```

No additional logging sink is required.

---

# Initial Implementation Scope

Implement:

```text
Startup Logs

Database Configuration Logs

Health Check Logs

Readiness Logs

Error Logs

Request Logs
```

Do not implement yet:

```text
Serilog

OpenTelemetry

Distributed Tracing

Correlation IDs

Custom Log Providers
```

These concerns will be introduced later when multiple services are available.

---

# Success Criteria

Logging implementation is considered complete when:

```text
✓ Application startup is visible in CloudWatch

✓ Incoming requests are visible in CloudWatch

✓ Database readiness failures are visible in CloudWatch

✓ Exceptions are visible in CloudWatch

✓ No sensitive information is logged
```

This logging strategy provides sufficient observability for the current single-service architecture while remaining simple and inexpensive to operate.
