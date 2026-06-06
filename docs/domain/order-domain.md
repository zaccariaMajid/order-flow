# Order Domain

## Purpose

The Order domain manages a customer's purchase request from creation through fulfillment outcome.
It owns the order lifecycle, order line snapshots, status transitions, and order-facing domain
events.

The Order aggregate is the consistency boundary for order business rules. It does not reserve
stock, process payments, create shipments, or send notifications. Those responsibilities belong to
the Inventory, Billing, Shipping, and Notification services.

## Aggregate Root

### Order

The `Order` aggregate represents a customer's intent to purchase one or more products.

#### Responsibilities

- Create an order from validated customer and item input.
- Maintain the current order lifecycle state.
- Store immutable item, quantity, and price snapshots.
- Calculate and expose the total order amount.
- Enforce valid status transitions.
- Record domain events for persistence through the Outbox pattern.

#### Invariants

- An order must have a customer.
- An order must contain at least one item.
- Item product identifiers must be unique within an order.
- Item quantities must be greater than zero.
- Item unit prices cannot be negative.
- Total amount must be greater than zero.
- Currency must be consistent across all order items.
- Customer and item snapshots cannot change after creation.
- Terminal orders cannot transition to another state.
- Cancelled, completed, and failed orders are terminal.

## Entities

### OrderItem

`OrderItem` is an entity inside the `Order` aggregate. It captures the purchased product snapshot at
the time the order is created.

#### Properties

| Property | Type | Description |
| --- | --- | --- |
| `Id` | `Guid` | Technical identifier for the order line. |
| `ProductId` | `ProductId` | Product identifier from the catalog context. |
| `Sku` | `Sku` | Business identifier for the purchased product variant. |
| `ProductName` | `string` | Product display name snapshot. |
| `Quantity` | `int` | Purchased quantity. |
| `UnitPrice` | `Money` | Product unit price snapshot. |
| `LineTotal` | `Money` | `Quantity * UnitPrice`. |

## Value Objects

| Value Object | Purpose | Key Rules |
| --- | --- | --- |
| `OrderId` | Identifies an order inside the platform. | Must wrap a non-empty `Guid`. |
| `CustomerId` | Identifies the customer placing the order. | Must wrap a non-empty `Guid`. |
| `ProductId` | Identifies a product. | Must wrap a non-empty `Guid`. |
| `Sku` | Identifies a product variant for operations and inventory. | Required, trimmed, normalized, and length-limited. |
| `Money` | Represents an amount and currency. | Amount cannot be negative; currency is required. |
| `FailureReason` | Describes why an order failed. | Required for failure transitions. |
| `CancellationReason` | Describes why an order was cancelled. | Required for cancellation transitions. |

## Order Status

Use a lifecycle that reflects cross-service choreography while keeping the Order service as the
state owner for the customer-facing order.

```text
Created
  |
  | OrderCreated integration event consumed by Inventory
  v
StockReserved
  |
  | StockReserved integration event consumed by Billing
  v
PaymentPending
  |
  +--> Failed
  |
  | PaymentCompleted integration event consumed by Shipping
  v
Paid
  |
  v
ShippingInProgress
  |
  v
Completed

Created
  |
  v
Cancelled

StockReserved
  |
  v
Cancelled
```

### Transition Rules

| Current | Command/Event | Next | Rule |
| --- | --- | --- | --- |
| None | `CreateOrder` | `Created` | Customer and at least one valid item are required. |
| `Created` | `StockReserved` | `StockReserved` | All requested stock must be reserved. |
| `Created` | `StockRejected` | `Failed` | Failure reason must include insufficient inventory details. |
| `StockReserved` | `BeginPayment` | `PaymentPending` | Payment can start only after stock is reserved. |
| `PaymentPending` | `PaymentCompleted` | `Paid` | Payment provider reference is required. |
| `PaymentPending` | `PaymentFailed` | `Failed` | Failure reason is required. |
| `Paid` | `ShipmentCreated` | `ShippingInProgress` | Shipment identifier is required. |
| `ShippingInProgress` | `ShipmentDelivered` | `Completed` | Delivery timestamp is required. |
| `Created`, `StockReserved` | `CancelOrder` | `Cancelled` | Cancellation reason is required. |

## Commands

Commands belong in the Application layer and invoke aggregate behavior. Command handlers must load
and persist aggregates through repository abstractions and persist domain events through the Outbox.

| Command | Purpose | Notes |
| --- | --- | --- |
| `CreateOrder` | Create a new customer order. | Validates customer, item list, quantity, price, and currency. |
| `CancelOrder` | Cancel an active order. | Allowed before payment completion. May require stock release. |
| `MarkStockReserved` | Apply successful stock reservation. | Triggered by `StockReserved` integration event. |
| `MarkStockRejected` | Fail order after inventory rejection. | Triggered by `StockRejected` integration event. |
| `BeginPayment` | Move the order into payment processing. | Internal transition after stock reservation. |
| `MarkPaymentCompleted` | Apply successful payment result. | Triggered by `PaymentCompleted` integration event. |
| `MarkPaymentFailed` | Fail order after payment failure. | Triggered by `PaymentFailed` integration event. |
| `MarkShipmentCreated` | Move order into shipping. | Triggered by `ShipmentCreated` integration event. |
| `MarkShipmentDelivered` | Complete delivered order. | Triggered by `ShipmentDelivered` integration event. |

## Queries

Queries must not mutate state. Prefer read models that are optimized for API use cases.

| Query | Purpose |
| --- | --- |
| `GetOrderById` | Return a single order and its item snapshots. |
| `GetCustomerOrders` | Return a customer's order history. |
| `GetOrdersByStatus` | Return orders by lifecycle state for operations. |
| `GetOpenOrders` | Return non-terminal orders requiring follow-up. |

## Domain Events

Domain events are raised by the aggregate and persisted with the order transaction. Integration
events may be mapped from these events by the Application or Infrastructure layer.

| Event | Raised When | Required Data |
| --- | --- | --- |
| `OrderCreated` | A valid order is created. | `OrderId`, `CustomerId`, `Items`, `TotalAmount`, `OccurredAt`. |
| `OrderCancelled` | An active order is cancelled. | `OrderId`, `Reason`, `OccurredAt`. |
| `OrderStockReserved` | Inventory reservation succeeds. | `OrderId`, `OccurredAt`. |
| `OrderPaymentStarted` | Payment processing begins. | `OrderId`, `OccurredAt`. |
| `OrderPaid` | Payment completes successfully. | `OrderId`, `PaymentId`, `OccurredAt`. |
| `OrderShippingStarted` | Shipment is created. | `OrderId`, `ShipmentId`, `OccurredAt`. |
| `OrderCompleted` | Shipment is delivered and the order is complete. | `OrderId`, `OccurredAt`. |
| `OrderFailed` | The order cannot proceed. | `OrderId`, `Reason`, `OccurredAt`. |

## Integration Events

The Order service publishes integration events for other services and consumes external outcomes.
Integration event contracts should live in shared contracts or service-specific contracts, not in
the Domain project.

### Publishes

- `OrderCreated`
- `OrderCancelled`
- `OrderCompleted`
- `OrderFailed`

### Consumes

- `StockReserved`
- `StockRejected`
- `PaymentCompleted`
- `PaymentFailed`
- `ShipmentCreated`
- `ShipmentDelivered`

## Persistence Model

The persistence model is owned by the Order service.

Recommended tables:

- `Orders`
- `OrderItems`
- `OutboxMessages`

Persistence requirements:

- Save aggregate changes and outbox messages in one transaction.
- Use UTC timestamps.
- Store monetary amounts with decimal precision appropriate for currency.
- Store currency explicitly.
- Add indexes for `CustomerId`, `Status`, and `CreatedAt`.
- Use optimistic concurrency when multiple event handlers can update the same order.

## API Boundaries

API endpoints should call Application commands and queries only. Controllers or endpoint handlers
must not enforce domain rules directly.

Recommended endpoints:

```text
POST   /orders
GET    /orders/{orderId}
GET    /customers/{customerId}/orders
POST   /orders/{orderId}/cancel
```

## Testing Requirements

Minimum test coverage:

- Creating an order with valid items succeeds.
- Creating an order without items fails.
- Creating an order with invalid quantity or price fails.
- Mixed item currencies are rejected.
- Total amount is calculated from item snapshots.
- Invalid lifecycle transitions are rejected.
- Terminal orders cannot be mutated.
- Expected domain events are recorded.
- Integration event handlers are idempotent.
- Concurrency conflicts do not silently overwrite newer order state.
