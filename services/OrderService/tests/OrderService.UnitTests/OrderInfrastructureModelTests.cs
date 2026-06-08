using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;

namespace OrderService.UnitTests;

public class OrderInfrastructureModelTests
{
    [Fact]
    public void Order_model_maps_aggregate_tables_and_ignores_domain_events()
    {
        using OrderDbContext context = CreateContext();
        IEntityType order = GetEntityType<Order>(context);
        IEntityType item = GetEntityType<OrderItem>(context);

        Assert.Equal("orders", order.GetTableName());
        Assert.Equal("order_items", item.GetTableName());
        Assert.Null(order.FindProperty(nameof(Order.DomainEvents)));
        Assert.NotNull(order.FindNavigation(nameof(Order.Items)));
        Assert.NotNull(item.FindProperty("OrderId"));
    }

    [Fact]
    public void Order_model_maps_money_as_owned_columns()
    {
        using OrderDbContext context = CreateContext();
        IEntityType order = GetEntityType<Order>(context);
        IEntityType item = GetEntityType<OrderItem>(context);

        Assert.NotNull(order.FindNavigation(nameof(Order.TotalAmount)));
        Assert.NotNull(item.FindNavigation(nameof(OrderItem.UnitPrice)));
        Assert.Contains(
            context.Model.GetEntityTypes(),
            entityType => entityType.ClrType.Name == "Money" && entityType.GetTableName() == "orders");
        Assert.Contains(
            context.Model.GetEntityTypes(),
            entityType => entityType.ClrType.Name == "Money" && entityType.GetTableName() == "order_items");
    }

    [Fact]
    public void Outbox_model_maps_json_content_and_indexes()
    {
        using OrderDbContext context = CreateContext();
        IEntityType outbox = GetEntityType<OutboxMessage>(context);

        Assert.Equal("outbox_messages", outbox.GetTableName());
        Assert.Equal("jsonb", outbox.FindProperty(nameof(OutboxMessage.Content))?.GetColumnType());
        Assert.Contains(outbox.GetIndexes(), index => index.Properties.Any(property => property.Name == nameof(OutboxMessage.ProcessedAt)));
        Assert.Contains(outbox.GetIndexes(), index => index.Properties.Any(property => property.Name == nameof(OutboxMessage.OccurredAt)));
    }

    private static OrderDbContext CreateContext()
    {
        DbContextOptions<OrderDbContext> options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql("Host=localhost;Database=order_db;Username=order;Password=order")
            .Options;

        return new OrderDbContext(options);
    }

    private static IEntityType GetEntityType<TEntity>(DbContext context) =>
        context.Model.FindEntityType(typeof(TEntity))
        ?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} was not mapped.");
}
