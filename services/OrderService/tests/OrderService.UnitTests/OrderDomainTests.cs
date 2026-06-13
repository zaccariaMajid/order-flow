using OrderService.Contracts;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.Exceptions;
using OrderService.Domain.Metadata;
using OrderService.Domain.ValueObjects;

namespace OrderService.UnitTests;

public class OrderDomainTests
{
    private static readonly CustomerId CustomerId = new(Guid.Parse("7d6b0912-3f65-48e2-a20f-3f950bd7d7c3"));
    private static readonly ProductId ProductId = new(Guid.Parse("b77d1a1e-8a65-42dd-839a-73f408cc2960"));

    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(OrderServiceDomain.Name, OrderServiceContracts.ServiceName);
        Assert.Contains("OrderCreated", OrderServiceContracts.PublishedEvents);
    }

    [Fact]
    public void Create_builds_pending_order_with_total_and_event()
    {
        DateTimeOffset createdAt = DateTimeOffset.Parse("2026-06-08T10:00:00Z");
        OrderItem firstItem = CreateItem(quantity: 2, amount: 10);
        OrderItem secondItem = CreateItem(quantity: 1, amount: 5);

        Order order = Order.Create(CustomerId, [firstItem, secondItem], createdAt);

        Assert.NotEqual(Guid.Empty, order.Id.Value);
        Assert.Equal(CustomerId, order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(25, order.TotalAmount.Amount);
        Assert.Equal(Currency.EUR, order.TotalAmount.Currency);
        Assert.Equal(createdAt, order.CreatedAt);
        OrderCreated created = Assert.IsType<OrderCreated>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, created.OrderId);
        Assert.Equal(order.TotalAmount, created.TotalAmount);
    }

    [Fact]
    public void Create_rejects_empty_items()
    {
        Assert.Throws<EmptyOrderException>(() => Order.Create(CustomerId, [], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_rejects_zero_total_order()
    {
        OrderItem item = CreateItem(quantity: 1, amount: 0);

        Assert.Throws<InvalidMoneyException>(() => Order.Create(CustomerId, [item], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_rejects_mixed_currency_items()
    {
        OrderItem eurItem = CreateItem(quantity: 1, amount: 10, currency: Currency.EUR);
        OrderItem usdItem = CreateItem(quantity: 1, amount: 10, currency: Currency.USD);

        Assert.Throws<InvalidMoneyException>(() => Order.Create(CustomerId, [eurItem, usdItem], DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Order_item_validates_required_fields()
    {
        Assert.Throws<ArgumentException>(() => OrderItem.Create(ProductId, " ", 1, new Money(10, Currency.EUR)));
        Assert.Throws<ArgumentOutOfRangeException>(() => OrderItem.Create(ProductId, "Product", 0, new Money(10, Currency.EUR)));
        Assert.Throws<ArgumentNullException>(() => OrderItem.Create(ProductId, "Product", 1, null!));
    }

    [Fact]
    public void Value_objects_validate_invalid_values()
    {
        Assert.Throws<ArgumentException>(() => new CustomerId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new OrderId(Guid.Empty));
        Assert.Throws<ArgumentException>(() => new ProductId(Guid.Empty));
        Assert.Throws<InvalidMoneyException>(() => new Money(-1, Currency.EUR));
        Assert.Throws<InvalidMoneyException>(() => new Money(1, (Currency)999));
    }

    [Fact]
    public void Order_follows_happy_path_state_machine()
    {
        DateTimeOffset completedAt = DateTimeOffset.Parse("2026-06-08T11:00:00Z");
        Order order = CreateOrder();

        order.MarkStockReserved();
        order.MarkAsPaid();
        order.StartShipping();
        order.Complete(completedAt);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(completedAt, order.CompletedAt);
        Assert.IsType<OrderCompleted>(order.DomainEvents.Last());
    }

    [Fact]
    public void Cancel_is_allowed_only_from_pending_and_publishes_event()
    {
        DateTimeOffset cancelledAt = DateTimeOffset.Parse("2026-06-08T12:00:00Z");
        Order order = CreateOrder();

        order.Cancel("customer request", cancelledAt);

        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(cancelledAt, order.CancelledAt);
        OrderCancelled cancelled = Assert.IsType<OrderCancelled>(order.DomainEvents.Last());
        Assert.Equal("customer request", cancelled.Reason);
        Assert.Throws<InvalidOrderStateException>(() => order.Complete(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Failed_order_cannot_be_completed()
    {
        Order order = CreateOrder();

        order.Fail("payment rejected", DateTimeOffset.UtcNow);

        Assert.Equal(OrderStatus.Failed, order.Status);
        Assert.IsType<OrderFailed>(order.DomainEvents.Last());
        Assert.Throws<InvalidOrderStateException>(() => order.Complete(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Invalid_state_transitions_are_rejected()
    {
        Order order = CreateOrder();

        Assert.Throws<InvalidOrderStateException>(() => order.MarkAsPaid());
        Assert.Throws<InvalidOrderStateException>(() => order.Complete(DateTimeOffset.UtcNow));
    }

    private static Order CreateOrder() => Order.Create(CustomerId, [CreateItem()], DateTimeOffset.UtcNow);

    private static OrderItem CreateItem(
        int quantity = 1,
        decimal amount = 10,
        Currency currency = Currency.EUR) =>
        OrderItem.Create(ProductId, "Product", quantity, new Money(amount, currency));
}
