# Ubiquitous Language

## Purpose

This document defines the shared vocabulary used across the Logistics Platform.

All code, documentation, APIs, events and database structures should use the terminology defined here.

The goal is to eliminate ambiguity and ensure a common understanding of the domain.

---

# Customer

A person or organization purchasing products through the platform.

### Notes

* A customer can place multiple orders.
* A customer owns the shipping destination.
* A customer is identified by a CustomerId.

---

# Product

A sellable item managed by the platform.

### Notes

* Products can be purchased through orders.
* Products are tracked by inventory.
* Products are identified by a ProductId.

---

# Product Variant

A specific variation of a product.

Examples:

* T-Shirt / Blue / XL
* Keyboard / Italian Layout
* Laptop / 32GB RAM

Each variant is tracked independently in inventory.

---

# SKU

Stock Keeping Unit.

Business identifier used to uniquely identify a product variant.

### Examples

```text
TSHIRT-BLUE-XL
KB-RGB-IT
LAPTOP-32GB
```

### Notes

* Used by inventory and warehouse operations.
* Human readable.
* Different from ProductId.

---

# ProductId

Technical identifier assigned by the platform.

### Notes

* Internal use only.
* Never used by warehouse operators.
* Not meaningful outside the system.

---

# Inventory

The collection of stock managed by the platform.

### Responsibilities

* Track stock availability.
* Track reserved stock.
* Prevent overselling.

---

# Inventory Item

A product tracked by the inventory system.

### Notes

* Represents current stock levels.
* Maintains reservation information.
* Identified by SKU.

---

# Available Stock

Quantity immediately available for reservation.

Formula:

```text
Available Stock =
Physical Stock - Reserved Stock
```

---

# Reserved Stock

Quantity temporarily allocated to orders.

### Notes

Reserved stock cannot be purchased by other customers.

---

# Stock Reservation

Temporary allocation of inventory for a specific order.

### Purpose

Prevent overselling while payment is being processed.

---

# Order

A customer's request to purchase one or more products.

### Lifecycle

```text
Created
Reserved
Paid
Shipped
Completed
```

or

```text
Created
Failed
```

---

# Order Item

A single purchased product inside an order.

### Notes

Contains:

* Product
* Quantity
* Price snapshot

---

# Payment

Financial transaction associated with an order.

### Notes

A payment may:

* Succeed
* Fail
* Be refunded

---

# Payment Authorization

Confirmation that payment can be processed.

### Notes

Authorization does not guarantee settlement.

---

# Payment Completion

Successful completion of a payment transaction.

### Notes

Triggers shipment creation.

---

# Shipment

Physical delivery process of an order.

### Responsibilities

* Package creation
* Tracking
* Delivery lifecycle

---

# Tracking Number

Unique identifier used to track a shipment.

### Example

```text
TRK-2026-000123
```

---

# Delivery

Final stage of shipment fulfillment.

### Notes

An order becomes Completed after successful delivery.

---

# Notification

Message sent to a customer regarding system activity.

### Channels

* Email
* SMS
* Push

---

# Aggregate

Cluster of domain objects treated as a single consistency boundary.

### Examples

* Order
* InventoryItem
* Payment
* Shipment

---

# Domain Event

Representation of something important that happened in the domain.

### Examples

```text
OrderCreated
StockReserved
PaymentCompleted
ShipmentDelivered
```

---

# Integration Event

Event published to other services.

### Notes

Usually derived from Domain Events.

---

# Outbox Message

Persisted integration event awaiting publication.

### Purpose

Guarantee reliable event delivery.

---

# Dead Letter Queue

Queue containing messages that could not be processed successfully.

### Purpose

Prevent message loss and enable troubleshooting.

---

# Correlation Id

Identifier used to track a business flow across multiple services.

### Example

```text
Customer places order
→ Inventory
→ Billing
→ Shipping
→ Notification
```

All operations share the same CorrelationId.

---

# Idempotency

Ability to process the same request multiple times without producing duplicate side effects.

### Example

```text
CreateOrder request retried twice

Result:
One order created
```

---

# Consistency Boundary

Area where business rules must remain immediately consistent.

### Examples

* Order Aggregate
* Inventory Aggregate

Consistency is not guaranteed across services.
