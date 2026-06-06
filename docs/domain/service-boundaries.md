# Service Boundaries

## Purpose

This document defines ownership, responsibilities and communication rules between services.

Each service owns its data and business rules.

No service is allowed to directly access another service's database.

Communication between services occurs through integration events.

---

# Architecture Overview

```text
┌─────────────────┐
│  Order Service  │
└────────┬────────┘
         │
         ▼
   OrderCreated
         │
         ▼
┌───────────────────┐
│ Inventory Service │
└─────────┬─────────┘
          │
          ├── StockReserved
          │
          └── StockRejected
                   │
                   ▼
┌─────────────────┐
│ Billing Service │
└────────┬────────┘
         │
         ├── PaymentCompleted
         │
         └── PaymentFailed
                  │
                  ▼
┌──────────────────┐
│ Shipping Service │
└────────┬─────────┘
         │
         ▼
  ShipmentCreated
         │
         ▼
┌──────────────────────┐
│ Notification Service │
└──────────────────────┘
```

---

# Order Service

## Purpose

Manages the lifecycle of customer orders.

Acts as the orchestration entry point.

---

## Owns

```text
Order
OrderItem
OrderStatus
```

---

## Database

```text
Orders
OrderItems
OutboxMessages
```

---

## Responsibilities

* Create orders
* Cancel orders
* Track order status
* Publish order events

---

## Publishes

```text
OrderCreated
OrderCancelled
```

---

## Consumes

```text
StockReserved
StockRejected

PaymentCompleted
PaymentFailed

ShipmentCreated
ShipmentDelivered
```

---

## Cannot

```text
Modify inventory

Modify stock

Process payments

Create shipments
```

---

# Inventory Service

## Purpose

Manages stock levels and reservations.

---

## Owns

```text
InventoryItem
StockReservation
```

---

## Database

```text
InventoryItems
StockReservations
OutboxMessages
```

---

## Responsibilities

* Reserve stock
* Release stock
* Validate availability
* Prevent overselling

---

## Publishes

```text
StockReserved
StockRejected
StockReleased
StockConsumed
```

---

## Consumes

```text
OrderCreated
OrderCancelled
```

---

## Cannot

```text
Create orders

Process payments

Create shipments
```

---

# Billing Service

## Purpose

Processes customer payments.

---

## Owns

```text
Payment
PaymentTransaction
```

---

## Database

```text
Payments
PaymentTransactions
OutboxMessages
```

---

## Responsibilities

* Authorize payments
* Capture payments
* Refund payments

---

## Publishes

```text
PaymentCompleted
PaymentFailed
PaymentRefunded
```

---

## Consumes

```text
StockReserved
```

---

## Cannot

```text
Reserve inventory

Create shipments

Modify orders
```

---

# Shipping Service

## Purpose

Manages shipment lifecycle.

---

## Owns

```text
Shipment
TrackingNumber
```

---

## Database

```text
Shipments
OutboxMessages
```

---

## Responsibilities

* Create shipments
* Track deliveries
* Update shipment status

---

## Publishes

```text
ShipmentCreated
ShipmentDelivered
ShipmentFailed
```

---

## Consumes

```text
PaymentCompleted
```

---

## Cannot

```text
Reserve stock

Modify orders

Process payments
```

---

# Notification Service

## Purpose

Sends customer notifications.

---

## Owns

```text
Notification
NotificationTemplate
```

---

## Database

```text
Notifications
NotificationTemplates
```

---

## Responsibilities

* Send emails
* Send SMS
* Send push notifications

---

## Publishes

```text
NotificationSent
NotificationFailed
```

---

## Consumes

```text
OrderCompleted
OrderFailed

ShipmentCreated
ShipmentDelivered

PaymentCompleted
PaymentFailed
```

---

# Data Ownership Rules

## Rule 1

A service owns its database.

Example:

```text
Inventory Service
      │
      ▼
Inventory Database
```

Only Inventory Service can write to it.

---

## Rule 2

Services communicate through events.

Allowed:

```text
OrderCreated
StockReserved
PaymentCompleted
```

Forbidden:

```text
InventoryService
    ↓
SELECT *
FROM Orders
```

---

## Rule 3

No shared database.

Forbidden:

```text
Order Service
Inventory Service
Billing Service

      ↓

Shared Database
```

---

## Rule 4

Cross-service consistency is eventual.

Immediate consistency exists only inside an aggregate.

Example:

```text
Order Aggregate
```

must always be consistent.

Example:

```text
Order + Inventory + Billing
```

may be eventually consistent.

---

# Ownership Matrix

| Resource         | Owner Service        |
| ---------------- | -------------------- |
| Order            | Order Service        |
| OrderItem        | Order Service        |
| InventoryItem    | Inventory Service    |
| StockReservation | Inventory Service    |
| Payment          | Billing Service      |
| Shipment         | Shipping Service     |
| Notification     | Notification Service |

---

# Integration Flow

```text
CreateOrder

↓ OrderCreated

ReserveStock

↓ StockReserved

ProcessPayment

↓ PaymentCompleted

CreateShipment

↓ ShipmentCreated

SendNotification
```

Every step is executed asynchronously through RabbitMQ.
