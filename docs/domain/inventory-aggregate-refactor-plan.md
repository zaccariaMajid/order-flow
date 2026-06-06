# Domain Model Aggregate Refactor Plan

## Architect Analysis

- Scope existing real domain models only: `Order` and `InventoryItem`.
- Treat Billing, Shipping, and Notification marker classes as missing domain models, not aggregates to invent in this refactor.
- Keep aggregate roots centered: `Order` owns `OrderItem`; `InventoryItem` owns `StockReservation`.
- Normalize domain abstractions through `BuildingBlocks.Domain` and remove duplicated per-service `IDomainEvent` and `DomainException`.

## Backend Implementation

- Shared domain:
  - Add `DomainException` to `BuildingBlocks.Domain`.
  - Change shared `IDomainEvent` to require only `DateTimeOffset OccurredAt`.
  - Reference `BuildingBlocks.Domain` from Order and Inventory domain projects.
- Order aggregate:
  - Group files under `Orders/Events`, `Orders/ValueObjects`, and `Orders/Entities`.
  - Use shared `IDomainEvent` and `DomainException`.
  - Add `MarkStockRejected` and `MarkPaymentFailed` to match the documented event choreography.
- Inventory aggregate:
  - Group files under `InventoryItems/Events`, `InventoryItems/ValueObjects`, `InventoryItems/Entities`, and `InventoryItems/Results`.
  - Keep `InventoryItem` as aggregate root and `StockReservation` as child entity.
  - Remove redundant product and SKU state from `StockReservation`.
  - Add `StockQuantity` and `AdjustmentReason` value objects.
  - Add consumed reservation lifecycle with `ConfirmStockConsumed` and `StockConsumed`.
  - Rename `ReservationResult` to `ReserveStockResult`.
  - Emit `LowStockThresholdReached` only when crossing into low-stock state.

## QA & Security Review

- Update Order tests for stock rejection, payment failure, and existing terminal transition rules.
- Update Inventory tests for reservation, release, consumption, duplicate reservation idempotency, stock quantity validation, adjustment reason validation, and low-stock threshold crossing.
- Run `dotnet build order-flow.slnx`.
- Run `dotnet test order-flow.slnx --no-build`.

## Assumptions

- No persistence, API, messaging, or application-layer handlers are added in this refactor.
- Billing, Shipping, and Notification aggregate creation is deferred to separate feature plans.
- Current primitive overloads may remain as compatibility wrappers when the core aggregate behavior uses value objects.
