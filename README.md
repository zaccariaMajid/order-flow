# Order Flow

Event-driven logistics and fulfillment platform skeleton built with ASP.NET Core, PostgreSQL, RabbitMQ, Redis, Clean Architecture, CQRS, Outbox, OpenTelemetry, Serilog, and unit testing in mind.

## Structure

- `services/` contains bounded service skeletons.
- `shared/` contains reusable building blocks, messaging contracts, and observability helpers.
- Each service keeps its own unit test project under `services/*/tests`.
- `docker/` and `docker-compose.yml` provide the local infrastructure baseline.

## Commands

```bash
dotnet build order-flow.slnx
dotnet test order-flow.slnx
docker compose up -d
```
