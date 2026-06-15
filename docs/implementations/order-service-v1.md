# Order Service V1 Implementation Plan

## Goal

Implement the first complete vertical slice of the Order Service.

The objective is to create and retrieve orders using:

* ASP.NET Core
* EF Core
* PostgreSQL (Neon)

External integrations are intentionally excluded.

Not included:

* RabbitMQ
* Outbox Pattern
* Inventory Service
* Billing Service
* Shipping Service
* Notifications

---

# Success Criteria

The implementation is complete when:

```text
POST /orders
```

creates an order successfully.

and

```text
GET /orders/{id}
```

returns the created order.

---

# Architecture

```text
HTTP Request
      │
      ▼
Controller / Endpoint
      │
      ▼
Command Handler
      │
      ▼
Order Aggregate
      │
      ▼
IOrderRepository
      │
      ▼
OrderRepository
      │
      ▼
OrderDbContext
      │
      ▼
PostgreSQL
```

---

# Phase 1

## Complete Domain Model

### Tasks

Implement:

```text
Order
OrderItem
OrderId
CustomerId
ProductId
Money
Currency
OrderStatus
```

---

### Domain Events

Implement:

```text
OrderCreated
OrderCancelled
OrderCompleted
OrderFailed
```

---

### Exceptions

Implement:

```text
DomainException
InvalidOrderStateException
EmptyOrderException
InvalidMoneyException
```

---

### Aggregate Behavior

Implement:

```text
Create()
Cancel()
MarkStockReserved()
MarkAsPaid()
StartShipping()
Complete()
Fail()
```

---

### Validation Rules

Verify:

```text
Order contains items

TotalAmount > 0

Valid state transitions
```

---

# Phase 2

## Unit Testing

Project:

```text
OrderService.Domain.Tests
```

---

### Tests

```text
Should_Create_Order

Should_Calculate_Total_Amount

Should_Throw_When_Order_Is_Empty

Should_Cancel_Order

Should_Not_Cancel_Completed_Order

Should_Raise_OrderCreated_Event

Should_Raise_OrderCancelled_Event
```

---

### Target

```text
100% aggregate behavior coverage
```

---

# Phase 3

## Persistence Layer

Project:

```text
OrderService.Infrastructure
```

---

## Create DbContext

File:

```text
Persistence/OrderDbContext.cs
```

DbSets:

```csharp
DbSet<Order>
DbSet<OrderItem>
```

---

## Entity Configurations

Create:

```text
Persistence/Configurations/

OrderConfiguration.cs

OrderItemConfiguration.cs
```

---

## Mapping Requirements

### Order

Table:

```text
orders
```

Columns:

```text
id
customer_id
status
total_amount
currency
created_at
completed_at
cancelled_at
```

---

### OrderItem

Table:

```text
order_items
```

Columns:

```text
id
order_id
product_id
product_name
quantity
unit_price
currency
```

---

### Value Object Conversions

Configure:

```text
OrderId

CustomerId

ProductId

Money

OrderStatus
```

using EF Core value converters.

---

# Phase 4

## PostgreSQL

Database:

```text
order_db
```

Provider:

```text
Npgsql
```

Host:

```text
Neon
```

---

## Connection String

Store using:

```text
User Secrets
```

or

```text
Environment Variables
```

Never commit credentials.

---

## Migration

Create:

```bash
dotnet ef migrations add InitialOrderSchema
```

Apply:

```bash
dotnet ef database update
```

---

# Phase 5

## Repository

Interface:

```csharp
IOrderRepository
```

Implementation:

```csharp
OrderRepository
```

Methods:

```csharp
Task<Order?> GetByIdAsync(
    OrderId id,
    CancellationToken ct);

Task AddAsync(
    Order order,
    CancellationToken ct);
```

---

# Phase 6

## Application Layer

Project:

```text
OrderService.Application
```

---

## Commands

### CreateOrderCommand

Properties:

```text
CustomerId

Items
```

---

### CreateOrderHandler

Responsibilities:

```text
Create aggregate

Persist aggregate

Save changes
```

---

## Queries

### GetOrderByIdQuery

Returns:

```text
OrderDetailsDto
```

---

### GetOrderByIdHandler

Uses:

```text
OrderDbContext
```

directly.

No repository required.

---

# Phase 7

## API Layer

Project:

```text
OrderService.Api
```

---

## Endpoint

### Create Order

```http
POST /orders
```

Request:

```json
{
  "customerId": "guid",
  "items": [
    {
      "productId": "guid",
      "productName": "Mechanical Keyboard",
      "quantity": 2,
      "unitPrice": 99.99
    }
  ]
}
```

Response:

```json
{
  "orderId": "guid"
}
```

---

### Get Order

```http
GET /orders/{id}
```

Response:

```json
{
  "id": "...",
  "customerId": "...",
  "status": "Pending",
  "totalAmount": 199.98,
  "items": []
}
```

---

# Phase 8

## Dependency Injection

Register:

```text
DbContext

Repository

Command Handlers

Query Handlers
```

inside:

```text
Program.cs
```

---

# Phase 9

## Validation

Use:

```text
FluentValidation
```

Create:

```text
CreateOrderValidator
```

Rules:

```text
CustomerId required

At least one item

Quantity > 0

UnitPrice > 0
```

---

# Phase 10

## Docker

Create:

```text
Dockerfile

.dockerignore
```

Verify:

```bash
docker build .
```

succeeds.

---

# Deliverable

At the end of V1 the following workflow must work:

```text
POST /orders
      │
      ▼
CreateOrderHandler
      │
      ▼
Order Aggregate
      │
      ▼
Repository
      │
      ▼
PostgreSQL
      │
      ▼
GET /orders/{id}
```

No RabbitMQ.

No Outbox.

No Inventory Service.

Only a complete and production-quality Order Service vertical slice.
