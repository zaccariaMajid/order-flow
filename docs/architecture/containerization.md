# Containerization Strategy

## Purpose

This document defines how services are packaged, deployed and executed.

Every service must be independently deployable.

Services are isolated and communicate only through APIs or integration events.

---

# Goals

* Independent deployment
* Environment portability
* Consistent runtime
* Reproducible builds
* Cloud readiness
* Local development support

---

# Deployment Model

Each service is packaged as a Docker container.

```text
Order Service
     │
     ▼
Docker Image

Inventory Service
     │
     ▼
Docker Image

Billing Service
     │
     ▼
Docker Image

Shipping Service
     │
     ▼
Docker Image

Notification Service
     │
     ▼
Docker Image
```

Every service can be deployed independently.

---

# Service Structure

Each service contains:

```text
InventoryService/

├── src/
├── tests/
├── Dockerfile
└── .dockerignore
```

---

# Dockerfile Strategy

Use multi-stage builds.

Benefits:

* Smaller images
* Faster deployments
* Reduced attack surface

---

# Build Stage

Responsibilities:

* Restore packages
* Build solution
* Run publish

Example:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore
RUN dotnet publish \
    -c Release \
    -o /app/publish
```

---

# Runtime Stage

Responsibilities:

* Execute application
* Expose HTTP endpoint

Example:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InventoryService.Api.dll"]
```

---

# Container Rules

## Rule 1

One service per container.

Allowed:

```text
Inventory Service Container
```

Forbidden:

```text
Inventory Service
RabbitMQ
Redis

inside same container
```

---

## Rule 2

Containers are stateless.

State belongs to:

```text
PostgreSQL
Redis
RabbitMQ
```

Never to the container filesystem.

---

## Rule 3

Configuration comes from environment variables.

Never hardcode:

```text
Connection Strings
Passwords
API Keys
```

inside Dockerfiles.

---

# Environment Variables

Every service supports:

```text
ASPNETCORE_ENVIRONMENT

ConnectionStrings__Database

RabbitMq__Host
RabbitMq__Port
RabbitMq__Username
RabbitMq__Password

OpenTelemetry__Endpoint
```

---

# Local Development

Docker Compose is used locally.

---

## Infrastructure Containers

```text
PostgreSQL
RabbitMQ
Redis
Jaeger
```

---

## Application Containers

```text
Order Service
Inventory Service
Billing Service
Shipping Service
Notification Service
```

---

# Local Architecture

```text
┌────────────────────┐
│ Order Service      │
└─────────┬──────────┘
          │
          ▼

┌────────────────────┐
│ RabbitMQ           │
└─────────┬──────────┘
          │

 ┌────────┼────────┐
 ▼        ▼        ▼

Inventory Billing Shipping

          │
          ▼

┌────────────────────┐
│ PostgreSQL         │
└────────────────────┘
```

---

# Docker Compose

The repository contains:

```text
docker-compose.yml
```

for local execution.

Responsibilities:

* Start infrastructure
* Start services
* Create internal network
* Configure dependencies

---

# Networking

All containers run in the same docker network.

Example:

```text
inventory-service
rabbitmq
postgres
redis
```

Services communicate through service names.

Example:

```text
Host=rabbitmq
```

instead of:

```text
Host=localhost
```

---

# Health Checks

Every service must expose:

```text
/health
```

and

```text
/health/ready
```

---

# Example

```text
GET /health
```

Response:

```json
{
  "status": "Healthy"
}
```

---

# Image Naming

Convention:

```text
logistics/order-service

logistics/inventory-service

logistics/billing-service

logistics/shipping-service

logistics/notification-service
```

---

# Versioning

Images use semantic versioning.

Example:

```text
1.0.0
1.1.0
2.0.0
```

Latest is never used in production.

---

# CI/CD Pipeline

Build process:

```text
Push
 │
 ▼

Restore

 ▼

Build

 ▼

Tests

 ▼

Docker Build

 ▼

Docker Push
```

---

# Deployment Readiness Checklist

A service is deployable only if:

* Builds successfully
* Unit tests pass
* Integration tests pass
* Docker image builds
* Health checks work
* Configuration comes from environment variables
* No secrets are hardcoded

---

# First Iteration Scope

Containerize:

```text
Inventory Service
```

only.

After Inventory Service is stable:

```text
Order Service
```

Then progressively containerize the remaining services.

Avoid managing 5 services simultaneously during the initial implementation.
