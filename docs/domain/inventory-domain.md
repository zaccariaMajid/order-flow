# Inventory Domain

## Purpose

The Inventory domain manages product availability and stock reservations. Its primary goal is to
prevent overselling while supporting concurrent order processing.

Inventory owns stock levels and reservation state. It does not create orders, process payments,
ship products, or notify customers.

## Aggregate Root

### InventoryItem

`InventoryItem` represents the stock state for one product variant.

#### Responsibilities

- Track physical stock.
- Track reserved stock.
- Calculate available stock.
- Reserve stock for an order.
- Release stock when an order is cancelled or fails.
- Reject reservations when available stock is insufficient.
- Record stock-related domain events.
- Protect stock changes with optimistic concurrency.

#### Invariants

- `ProductId` is required.
- `Sku` is required.
- Physical stock cannot be negative.
- Reserved stock cannot be negative.
- Reserved stock cannot exceed physical stock.
- Available stock cannot be negative.
- Reservation quantity must be greater than zero.
- A reservation must reference an order.
- An order cannot hold more than one active reservation for the same inventory item.
- Released, consumed, or rejected reservations cannot be released again.

## Entities

### StockReservation

`StockReservation` is an entity inside the `InventoryItem` aggregate. It represents stock allocated
to a specific order while the order is being completed.

#### Properties

| Property | Type | Description |
| --- | --- | --- |
| `Id` | `Guid` | Reservation identifier. |
| `OrderId` | `OrderId` | Order that requested the stock. |
| `Quantity` | `int` | Reserved quantity. |
| `Status` | `ReservationStatus` | Current reservation state. |
| `CreatedAt` | `DateTimeOffset` | Creation timestamp in UTC. |
| `ReleasedAt` | `DateTimeOffset?` | Release timestamp in UTC. |
| `ConsumedAt` | `DateTimeOffset?` | Consumption timestamp in UTC. |

`ProductId` and `Sku` are owned by the `InventoryItem` aggregate root and are not duplicated on
`StockReservation`.

## Value Objects

| Value Object | Purpose | Key Rules |
| --- | --- | --- |
| `ProductId` | Identifies a product. | Must wrap a non-empty `Guid`. |
| `Sku` | Identifies a product variant. | Required, trimmed, normalized, and length-limited. |
| `OrderId` | Identifies the order requesting stock. | Must wrap a non-empty `Guid`. |
| `StockQuantity` | Represents a stock quantity. | Cannot be negative for stock levels; must be positive for reservations. |
| `ReservationId` | Identifies a reservation. | Must wrap a non-empty `Guid`. |
| `AdjustmentReason` | Describes why physical stock changed. | Required and trimmed. |

## Stock Formula

Available stock is derived from physical and reserved stock:

```text
AvailableStock = PhysicalStock - ReservedStock
```

Do not persist `AvailableStock` as the source of truth unless the persistence model needs a
read-optimized projection. If persisted, it must be treated as derived data and updated atomically.

## Reservation Lifecycle

```text
Requested
  |
  +--> Reserved
  |      |
  |      +--> Released
  |      |
  |      +--> Consumed
  |
  +--> Rejected
```

### Transition Rules

| Current | Command/Event | Next | Rule |
| --- | --- | --- | --- |
| None | `ReserveStock` | `Reserved` | Available stock must be greater than or equal to requested quantity. |
| None | `ReserveStock` | `Rejected` | Requested quantity exceeds available stock. |
| `Reserved` | `ReleaseStock` | `Released` | Reservation exists and has not already been released. |
| `Reserved` | `ConfirmStockConsumed` | `Consumed` | Payment and fulfillment have advanced far enough that stock is no longer temporary. |

The initial `Requested` state can be represented as an application-level request rather than a
persisted reservation row. Persist only meaningful domain outcomes when that keeps the model simpler.

## Commands

Commands belong in the Application layer and invoke aggregate behavior. Command handlers must load
and persist aggregates through repository abstractions and persist domain events through the Outbox.

| Command | Purpose | Notes |
| --- | --- | --- |
| `ReserveStock` | Reserve product quantity for an order. | Triggered by `OrderCreated` integration event. |
| `ReleaseStock` | Return reserved stock to availability. | Triggered by `OrderCancelled`, `PaymentFailed`, or expiration workflows. |
| `AdjustPhysicalStock` | Increase or decrease physical stock. | Used by operational stock corrections or receiving workflows. |
| `SetReorderThreshold` | Change low-stock alert threshold. | Operational command. |
| `ConfirmStockConsumed` | Finalize reserved stock as consumed. | Used when fulfillment passes the point where stock should no longer be released. |

## Queries

Queries must not mutate state. Prefer read models optimized for operations and availability checks.

| Query | Purpose |
| --- | --- |
| `GetInventoryItem` | Return inventory state for one product variant. |
| `GetInventoryAvailability` | Return physical, reserved, and available stock. |
| `GetLowStockItems` | Return items at or below reorder threshold. |
| `GetReservationsByOrderId` | Return reservations linked to an order. |

## Domain Events

Domain events are raised by the aggregate and persisted with the inventory transaction.

| Event | Raised When | Required Data |
| --- | --- | --- |
| `StockReserved` | Requested stock is successfully reserved. | `ReservationId`, `OrderId`, `ProductId`, `Sku`, `Quantity`, `OccurredAt`. |
| `StockRejected` | Available stock is insufficient. | `OrderId`, `ProductId`, `Sku`, `RequestedQuantity`, `AvailableQuantity`, `OccurredAt`. |
| `StockReleased` | Reserved stock is returned to availability. | `ReservationId`, `OrderId`, `ProductId`, `Sku`, `Quantity`, `OccurredAt`. |
| `StockConsumed` | Reserved stock is confirmed as consumed. | `ReservationId`, `OrderId`, `ProductId`, `Sku`, `Quantity`, `OccurredAt`. |
| `PhysicalStockAdjusted` | Physical stock changes. | `ProductId`, `Sku`, `PreviousQuantity`, `NewQuantity`, `Reason`, `OccurredAt`. |
| `LowStockThresholdReached` | Available stock crosses from above threshold to at or below threshold. | `ProductId`, `Sku`, `AvailableQuantity`, `Threshold`, `OccurredAt`. |

## Integration Events

The Inventory service publishes inventory outcomes for downstream services and consumes order
lifecycle events. Integration event contracts should live in shared contracts or service-specific
contracts, not in the Domain project.

### Publishes

- `StockReserved`
- `StockRejected`
- `StockReleased`
- `StockConsumed`
- `LowStockThresholdReached`

### Consumes

- `OrderCreated`
- `OrderCancelled`
- `PaymentFailed`
- `ShipmentCreated` or a fulfillment event that confirms stock consumption

## Concurrency Strategy

Inventory must assume concurrent reservation attempts for the same product.

Use optimistic concurrency by default:

- Add a concurrency token such as `Version`, `RowVersion`, or provider-specific `xmin`.
- Check the token on every stock-changing write.
- Retry only when the command is idempotent and the aggregate can be reloaded safely.
- Return a rejected reservation outcome when stock is no longer available after reload.
- Keep reservation commands idempotent by using `OrderId` and `ProductId` as a natural duplicate
  guard for active reservations.

The failure mode to prevent:

```text
AvailableStock = 1

Customer A reserves 1 item
Customer B reserves 1 item concurrently

Invalid result:
ReservedStock = 2
AvailableStock = -1
```

## Persistence Model

The persistence model is owned by the Inventory service.

Recommended tables:

- `InventoryItems`
- `StockReservations`
- `OutboxMessages`

Persistence requirements:

- Save aggregate changes and outbox messages in one transaction.
- Use UTC timestamps.
- Add a unique active-reservation constraint for `OrderId` and `ProductId`.
- Add indexes for `Sku`, `ProductId`, `AvailableStock` projection, and reservation `OrderId`.
- Store concurrency token on `InventoryItems`.
- Do not allow direct writes from other services.

## API Boundaries

API endpoints should call Application commands and queries only. Endpoint handlers must not enforce
inventory rules directly.

Recommended endpoints:

```text
GET    /inventory/{sku}
GET    /inventory/{sku}/availability
POST   /inventory/{sku}/adjustments
POST   /inventory/{sku}/reservations
POST   /inventory/{sku}/reservations/{reservationId}/release
```

Reservation endpoints are useful for internal testing and operations. In the normal order flow,
reservation happens by consuming `OrderCreated`.

## Testing Requirements

Minimum test coverage:

- Reserving stock succeeds when available stock is sufficient.
- Reserving stock fails when requested quantity exceeds available stock.
- Physical stock cannot become negative.
- Reserved stock cannot exceed physical stock.
- Releasing a reservation restores available stock.
- Releasing a missing, already released, or consumed reservation fails safely.
- Duplicate reservation commands for the same order and product are idempotent.
- Concurrent reservations cannot oversell stock.
- Expected domain events are recorded.
- Integration event handlers are idempotent.
- Outbox messages are persisted with stock changes.
