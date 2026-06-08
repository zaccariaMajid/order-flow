# Persistence Strategy

## Purpose

This document defines how persistence is implemented across the Logistics Platform.

Each service owns its own PostgreSQL database hosted on Neon.

Entity Framework Core is used as ORM.

---

# Database Strategy

The system follows the Database per Service pattern.

Each service owns its persistence model and database schema.

```text
Order Service       → order_db
Inventory Service   → inventory_db
Billing Service     → billing_db
Shipping Service    → shipping_db
Notification Service → notification_db
```

A service is not allowed to directly access another service's database.

Cross-service communication happens through integration events.

---

# Technology Stack

* PostgreSQL
* Neon
* Entity Framework Core
* Npgsql
* EF Core Migrations
* Outbox Pattern

---

# DbContext Location

Each service contains its own DbContext inside the Infrastructure layer.

Example:

```text
InventoryService/

└── src/
    ├── InventoryService.Domain/
    ├── InventoryService.Application/
    ├── InventoryService.Infrastructure/
    │   └── Persistence/
    │       ├── InventoryDbContext.cs
    │       ├── Configurations/
    │       │   ├── InventoryItemConfiguration.cs
    │       │   └── StockReservationConfiguration.cs
    │       └── Migrations/
    └── InventoryService.Api/
```

---

# DbContext Responsibilities

The DbContext is responsible for:

* Mapping domain entities to PostgreSQL tables
* Applying entity configurations
* Managing transactions
* Persisting aggregate state
* Persisting Outbox messages
* Enforcing optimistic concurrency

The DbContext must not contain business logic.

---

# InventoryDbContext

```csharp
public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

---

# Connection String

Connection strings are stored in configuration and never committed to source control.

Example:

```json
{
  "ConnectionStrings": {
    "InventoryDb": "Host=your-neon-host;Database=inventory_db;Username=your-user;Password=your-password;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

For local development, use:

```text
dotnet user-secrets
```

or environment variables.

Example environment variable:

```text
ConnectionStrings__InventoryDb
```

---

# Dependency Injection

The DbContext is registered in the API project.

```csharp
builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("InventoryDb"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        });
});
```

---

# Entity Configuration

Entity mappings are implemented using `IEntityTypeConfiguration<T>`.

This keeps the DbContext clean and separates persistence configuration from domain logic.

---

## InventoryItemConfiguration

```csharp
public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.ProductId)
            .HasConversion(
                productId => productId.Value,
                value => new ProductId(value))
            .HasColumnName("product_id");

        builder.Property(x => x.Sku)
            .HasConversion(
                sku => sku.Value,
                value => new Sku(value))
            .HasColumnName("sku")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PhysicalStock)
            .HasColumnName("physical_stock")
            .IsRequired();

        builder.Property(x => x.ReservedStock)
            .HasColumnName("reserved_stock")
            .IsRequired();

        builder.Property(x => x.ReorderThreshold)
            .HasColumnName("reorder_threshold")
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken();

        builder.HasIndex(x => x.Sku)
            .IsUnique();
    }
}
```

---

## StockReservationConfiguration

```csharp
public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                reservationId => reservationId.Value,
                value => new ReservationId(value))
            .HasColumnName("id");

        builder.Property(x => x.OrderId)
            .HasConversion(
                orderId => orderId.Value,
                value => new OrderId(value))
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasConversion(
                productId => productId.Value,
                value => new ProductId(value))
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => new { x.OrderId, x.ProductId })
            .IsUnique();
    }
}
```

---

# Outbox Table

Each service contains an Outbox table.

The Outbox table stores integration events before they are published to RabbitMQ.

This prevents data loss when the database transaction succeeds but message publishing fails.

---

## OutboxMessage

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public string Type { get; private set; }

    public string Content { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public DateTime? ProcessedAt { get; private set; }

    public string? Error { get; private set; }
}
```

---

## OutboxMessageConfiguration

```csharp
public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnName("content")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(x => x.ProcessedAt)
            .HasColumnName("processed_at");

        builder.Property(x => x.Error)
            .HasColumnName("error");

        builder.HasIndex(x => x.ProcessedAt);

        builder.HasIndex(x => x.OccurredAt);
    }
}
```

---

# Transactions

Domain state changes and Outbox messages must be saved in the same database transaction.

Example:

```text
Transaction
├── Update InventoryItem
├── Create StockReservation
└── Insert OutboxMessage
```

If the transaction fails, nothing is persisted.

If the transaction succeeds, the event is guaranteed to exist in the Outbox table.

---

# Migrations

Each service manages its own EF Core migrations.

Example command:

```bash
dotnet ef migrations add InitialInventorySchema \
  --project services/InventoryService/src/InventoryService.Infrastructure \
  --startup-project services/InventoryService/src/InventoryService.Api \
  --context InventoryDbContext
```

Apply migration:

```bash
dotnet ef database update \
  --project services/InventoryService/src/InventoryService.Infrastructure \
  --startup-project services/InventoryService/src/InventoryService.Api \
  --context InventoryDbContext
```

---

# Neon Considerations

Neon databases require SSL.

The connection string must include:

```text
SSL Mode=Require
```

Recommended practices:

* Store connection strings in secrets
* Use separate databases per service
* Use connection pooling carefully
* Avoid long-running transactions
* Keep migrations service-specific

---

# Naming Conventions

Database objects use snake_case.

Examples:

```text
inventory_items
stock_reservations
outbox_messages
physical_stock
reserved_stock
created_at
```

C# code uses PascalCase.

Examples:

```csharp
InventoryItem
StockReservation
PhysicalStock
ReservedStock
CreatedAt
```

---

# Rules

## Rule 1

Domain entities must not depend on Entity Framework Core.

---

## Rule 2

Persistence configuration belongs in Infrastructure.

---

## Rule 3

The DbContext must not contain business logic.

---

## Rule 4

Each service owns its own database.

---

## Rule 5

Integration events must be persisted through the Outbox Pattern before being published.

---

# First Implementation Scope

For the first iteration, implement persistence only for:

* Order
