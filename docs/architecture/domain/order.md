# Order Domain

## Purpose

The Order domain is responsible for managing the lifecycle of a customer's purchase request.

An Order represents the intention of a customer to purchase one or more products.

The aggregate acts as the consistency boundary for all order-related business rules.

---

# Aggregate Root

## Order

### Responsibilities

* Create orders
* Track order state
* Calculate total amount
* Prevent invalid state transitions
* Publish domain events

### Invariants

* An order must contain at least one item.
* Total amount must be greater than zero.
* Customer cannot be changed after creation.
* Completed orders are immutable.
* Cancelled orders cannot be completed.
* Failed orders cannot be completed.

---

# Properties

| Property    | Type                           | Description                        |
| ----------- | ------------------------------ | ---------------------------------- |
| Id          | Guid                           | Technical identifier of the order. |
| CustomerId  | CustomerId                     | Customer who placed the order.     |
| Items       | IReadOnlyCollection<OrderItem> | Purchased products snapshot.       |
| Status      | OrderStatus                    | Current lifecycle state.           |
| TotalAmount | Money                          | Total order value.                 |
| CreatedAt   | DateTime                       | Creation timestamp (UTC).          |
| CompletedAt | DateTime?                      | Completion timestamp (UTC).        |
| CancelledAt | DateTime?                      | Cancellation timestamp (UTC).      |

---

# Entity

## OrderItem

Represents a purchased product inside an order.

### Invariants

* Quantity must be greater than zero.
* Unit price cannot be negative.
* Product name is immutable.

### Properties

| Property    | Type      | Description            |
| ----------- | --------- | ---------------------- |
| Id          | Guid      | Technical identifier.  |
| ProductId   | ProductId | Purchased product.     |
| ProductName | string    | Product name snapshot. |
| Quantity    | int       | Purchased quantity.    |
| UnitPrice   | Money     | Product unit price.    |
| TotalPrice  | Money     | Quantity × UnitPrice.  |

---

# Value Objects

## CustomerId

Business identifier of a customer.

```csharp
public record CustomerId(Guid Value);
```

---

## ProductId

Business identifier of a product.

```csharp
public record ProductId(Guid Value);
```

---

## Money

Represents a monetary amount.

### Invariants

* Amount >= 0
* Currency must be specified

```csharp
public record Money
{
    decimal Amount;
    Currency Currency;
}
```

---

# Order State Machine

```text
Pending
   │
   ▼
StockReserved
   │
   ▼
PaymentPending
   │
   ├────────► Failed
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
```

---

# Domain Events

## OrderCreated

Raised when a new order is successfully created.

### Payload

```text
OrderId
CustomerId
Items
TotalAmount
OccurredAt
```

---

## OrderCancelled

Raised when an order is cancelled.

### Payload

```text
OrderId
Reason
OccurredAt
```

---

## OrderCompleted

Raised when an order reaches its terminal successful state.

### Payload

```text
OrderId
OccurredAt
```

---

## OrderFailed

Raised when the order cannot proceed.

### Payload

```text
OrderId
Reason
OccurredAt
```

---

# Commands

```text
CreateOrder
CancelOrder
MarkOrderAsPaid
MarkOrderAsCompleted
FailOrder
```

---

# Queries

```text
GetOrderById
GetCustomerOrders
GetOrdersByStatus
```
