# Order Aggregate Design

## Purpose

This document extends the Order Domain specification and defines the missing components required before implementing the Application Layer.

The goal is to ensure that the Order aggregate is fully modeled, testable and persistence-ready.

---

# Missing Value Objects

## OrderId

The Order aggregate should expose a business identifier instead of a raw Guid.

Current:

```csharp
public Guid Id { get; }
```

Target:

```csharp
public OrderId Id { get; }
```

Implementation:

```csharp
public sealed record OrderId(Guid Value);
```

---

# Aggregate Structure

```text
Order
│
├── OrderId
├── CustomerId
├── Status
├── TotalAmount
├── CreatedAt
├── CompletedAt
├── CancelledAt
│
└── OrderItems
    │
    ├── ProductId
    ├── ProductName
    ├── Quantity
    ├── UnitPrice
    └── TotalPrice
```

---

# Aggregate Lifecycle

## Creation

Order starts in:

```text
Pending
```

Domain event raised:

```text
OrderCreated
```

---

## Inventory Reservation

Order transitions to:

```text
StockReserved
```

Domain event raised:

```text
StockReserved
```

---

## Payment Completed

Order transitions to:

```text
Paid
```

Domain event raised:

```text
OrderPaid
```

---

## Shipment Started

Order transitions to:

```text
ShippingInProgress
```

Domain event raised:

```text
ShipmentStarted
```

---

## Completion

Order transitions to:

```text
Completed
```

Domain event raised:

```text
OrderCompleted
```

---

## Failure

Order transitions to:

```text
Failed
```

Domain event raised:

```text
OrderFailed
```

---

## Cancellation

Order transitions to:

```text
Cancelled
```

Domain event raised:

```text
OrderCancelled
```

---

# State Machine

```text
Pending
   │
   ▼
StockReserved
   │
   ▼
Paid
   │
   ▼
ShippingInProgress
   │
   ▼
Completed

Pending
   │
   ▼
Cancelled

Pending
StockReserved
Paid
ShippingInProgress
   │
   ▼
Failed
```

---

# Aggregate Methods

The aggregate should expose behavior instead of allowing state mutations.

---

## Create

```csharp
public static Order Create(
    CustomerId customerId,
    IReadOnlyCollection<OrderItem> items)
```

Responsibilities:

* Validate items
* Calculate total amount
* Initialize state
* Raise OrderCreated

---

## Cancel

```csharp
public void Cancel(string reason)
```

Responsibilities:

* Validate transition
* Update state
* Set CancelledAt
* Raise OrderCancelled

---

## MarkStockReserved

```csharp
public void MarkStockReserved()
```

Responsibilities:

* Validate transition
* Update state

---

## MarkAsPaid

```csharp
public void MarkAsPaid()
```

Responsibilities:

* Validate transition
* Update state

---

## StartShipping

```csharp
public void StartShipping()
```

Responsibilities:

* Validate transition
* Update state

---

## Complete

```csharp
public void Complete()
```

Responsibilities:

* Validate transition
* Set CompletedAt
* Raise OrderCompleted

---

## Fail

```csharp
public void Fail(string reason)
```

Responsibilities:

* Validate transition
* Update state
* Raise OrderFailed

---

# Domain Exceptions

## DomainException

Base exception for domain violations.

```csharp
public abstract class DomainException : Exception
{
}
```

---

## InvalidOrderStateException

Raised when an invalid state transition is attempted.

Examples:

```text
Completed → Cancelled

Cancelled → Completed

Failed → Paid
```

---

## EmptyOrderException

Raised when creating an order without items.

---

## InvalidMoneyException

Raised when total amount is invalid.

---

# Repository Contract

The repository contract belongs to the Domain layer.

```csharp
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(
        OrderId orderId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Order order,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Order order,
        CancellationToken cancellationToken);
}
```

---

# Domain Event Contract

All domain events must inherit from a common abstraction.

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTime OccurredAt { get; }
}
```

---

# Aggregate Event Collection

The aggregate should internally track generated domain events.

Example:

```csharp
private readonly List<IDomainEvent> _domainEvents = [];

public IReadOnlyCollection<IDomainEvent> DomainEvents =>
    _domainEvents.AsReadOnly();
```

Purpose:

* Unit testing
* Outbox integration
* Event publishing

---

# Unit Tests Required

## Order Creation

```text
Should_Create_Order
```

---

## Empty Order

```text
Should_Throw_When_Order_Has_No_Items
```

---

## Total Calculation

```text
Should_Calculate_Total_Amount
```

---

## Cancellation

```text
Should_Cancel_Order
```

---

## Invalid State Transition

```text
Should_Not_Cancel_Completed_Order
```

---

## Event Emission

```text
Should_Raise_OrderCreated_Event

Should_Raise_OrderCancelled_Event

Should_Raise_OrderCompleted_Event
```

---

# Ready For Application Layer

The aggregate is considered complete when:

* All invariants are enforced
* All transitions are validated
* Domain events are raised
* Unit tests pass
* Repository contract exists
* No EF Core dependency exists in Domain

```
```
