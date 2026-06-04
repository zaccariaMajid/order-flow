# Logistics Platform - Repository Structure

## Overview

Event-driven logistics and fulfillment platform built to demonstrate enterprise backend engineering skills:

* ASP.NET Core
* PostgreSQL
* RabbitMQ
* Redis
* Docker
* Clean Architecture
* CQRS
* Outbox Pattern
* OpenTelemetry
* Serilog
* Integration Testing
* GitHub Actions

---

# Repository Structure

```text
logistics-platform/

├── docs/
│   ├── architecture/
│   │   ├── system-context.md
│   │   ├── service-boundaries.md
│   │   └── event-flow.md
│   │
│   ├── adr/
│   │   ├── ADR-001-clean-architecture.md
│   │   ├── ADR-002-rabbitmq.md
│   │   ├── ADR-003-outbox-pattern.md
│   │   └── ADR-004-redis-cache.md
│   │
│   └── diagrams/
│       ├── architecture.png
│       ├── event-flow.png
│       └── deployment.png
│
├── docker/
│   ├── postgres/
│   ├── rabbitmq/
│   ├── redis/
│   └── monitoring/
│
├── services/
│
│   ├── OrderService/
│   │
│   │   ├── src/
│   │   │
│   │   │   ├── OrderService.Api/
│   │   │   ├── OrderService.Application/
│   │   │   ├── OrderService.Domain/
│   │   │   ├── OrderService.Infrastructure/
│   │   │   └── OrderService.Contracts/
│   │   │
│   │   └── tests/
│   │       ├── OrderService.UnitTests/
│   │       └── OrderService.IntegrationTests/
│   │
│   ├── InventoryService/
│   │
│   │   ├── src/
│   │   │   ├── InventoryService.Api/
│   │   │   ├── InventoryService.Application/
│   │   │   ├── InventoryService.Domain/
│   │   │   ├── InventoryService.Infrastructure/
│   │   │   └── InventoryService.Contracts/
│   │   │
│   │   └── tests/
│   │       ├── InventoryService.UnitTests/
│   │       └── InventoryService.IntegrationTests/
│   │
│   ├── BillingService/
│   │
│   │   ├── src/
│   │   │   ├── BillingService.Api/
│   │   │   ├── BillingService.Application/
│   │   │   ├── BillingService.Domain/
│   │   │   ├── BillingService.Infrastructure/
│   │   │   └── BillingService.Contracts/
│   │   │
│   │   └── tests/
│   │       ├── BillingService.UnitTests/
│   │       └── BillingService.IntegrationTests/
│   │
│   ├── ShippingService/
│   │
│   │   ├── src/
│   │   │   ├── ShippingService.Api/
│   │   │   ├── ShippingService.Application/
│   │   │   ├── ShippingService.Domain/
│   │   │   ├── ShippingService.Infrastructure/
│   │   │   └── ShippingService.Contracts/
│   │   │
│   │   └── tests/
│   │       ├── ShippingService.UnitTests/
│   │       └── ShippingService.IntegrationTests/
│   │
│   └── NotificationService/
│
│       ├── src/
│       │   ├── NotificationService.Api/
│       │   ├── NotificationService.Application/
│       │   ├── NotificationService.Domain/
│       │   ├── NotificationService.Infrastructure/
│       │   └── NotificationService.Contracts/
│       │
│       └── tests/
│           ├── NotificationService.UnitTests/
│           └── NotificationService.IntegrationTests/
│
├── shared/
│
│   ├── BuildingBlocks/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── Api/
│   │
│   ├── Messaging/
│   │   ├── RabbitMq/
│   │   ├── Outbox/
│   │   └── Consumers/
│   │
│   ├── Contracts/
│   │   ├── Orders/
│   │   ├── Inventory/
│   │   ├── Billing/
│   │   ├── Shipping/
│   │   └── Notifications/
│   │
│   └── Observability/
│       ├── Logging/
│       ├── Tracing/
│       └── Metrics/
│
├── tests/
│
│   ├── EndToEndTests/
│   │
│   ├── ContractTests/
│   │
│   └── TestUtilities/
│
├── scripts/
│   ├── start-local.ps1
│   ├── stop-local.ps1
│   ├── migrate-db.ps1
│   └── seed-data.ps1
│
├── .github/
│   └── workflows/
│       ├── build.yml
│       ├── tests.yml
│       └── docker.yml
│
├── docker-compose.yml
├── README.md
└── logistics-platform.slnx
```

---

# Event Flow

```text
Order Created
      │
      ▼
Inventory Service
      │
      ├── Stock Reserved
      │         │
      │         ▼
      │   Billing Service
      │         │
      │         ├── Payment Completed
      │         │         │
      │         │         ▼
      │         │   Shipping Service
      │         │         │
      │         │         ▼
      │         │   Shipment Created
      │         │         │
      │         │         ▼
      │         │ Notification Service
      │         │
      │         ▼
      │   Order Completed
      │
      └── Stock Rejected
                │
                ▼
          Order Failed
```

---

# Order Service Responsibilities

* Create Order
* Cancel Order
* Track Order Status
* Publish Order Events
* Persist Outbox Messages

Events:

* OrderCreated
* OrderCancelled
* OrderCompleted
* OrderFailed

---

# Inventory Service Responsibilities

* Stock Management
* Reservation Management
* Availability Validation

Events:

* StockReserved
* StockRejected

---

# Billing Service Responsibilities

* Simulated Payment Processing
* Retry Logic
* Payment Status Tracking

Events:

* PaymentCompleted
* PaymentFailed

---

# Shipping Service Responsibilities

* Shipment Creation
* Shipment Tracking
* Delivery Status

Events:

* ShipmentCreated
* ShipmentDelivered

---

# Notification Service Responsibilities

* Email Notifications
* SMS Notifications
* Push Notifications

Events Consumed:

* OrderCompleted
* OrderFailed
* ShipmentCreated
* ShipmentDelivered

---

# Cross-Cutting Concerns

## Redis

* Inventory cache
* Product cache
* Distributed locks

## RabbitMQ

* Event bus
* Dead letter queues
* Retry queues

## PostgreSQL

Each service owns its own database schema.

## Outbox Pattern

Every domain event:

1. Saved in database
2. Saved in Outbox table
3. Published by background worker
4. Marked as processed

## OpenTelemetry

* Distributed tracing
* Service correlation
* Request diagnostics

## Serilog

Structured logging with:

* CorrelationId
* TraceId
* UserId
* ServiceName

---

# Future Enhancements

* API Gateway (YARP)
* Kubernetes deployment
* Saga Orchestrator
* Temporal.io
* Event Sourcing
* CQRS Read Models
* Grafana Dashboards
* Prometheus Metrics

```
```
