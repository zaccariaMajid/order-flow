using BuildingBlocks.Domain;
using OrderService.Domain.Orders;

namespace OrderService.UnitTests.Orders;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 01, 01, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidItems_CreatesOrderAndRecordsEvent()
    {
        var order = CreateOrder();

        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(20m, order.TotalAmount.Amount);
        Assert.Equal("EUR", order.TotalAmount.Currency);
        Assert.Single(order.DomainEvents);
        Assert.IsType<OrderCreated>(order.DomainEvents.Single());
    }

    [Fact]
    public void Create_WithoutItems_Fails()
    {
        var exception = Assert.Throws<DomainException>(() =>
            Order.Create(NewOrderId(), NewCustomerId(), [], Now));

        Assert.Equal("An order must contain at least one item.", exception.Message);
    }

    [Fact]
    public void Create_WithMixedCurrencies_Fails()
    {
        var items = new[]
        {
            NewItem(productId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), currency: "EUR"),
            NewItem(productId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), currency: "USD")
        };

        var exception = Assert.Throws<DomainException>(() =>
            Order.Create(NewOrderId(), NewCustomerId(), items, Now));

        Assert.Equal("All order items must use the same currency.", exception.Message);
    }

    [Fact]
    public void Create_WithDuplicateProducts_Fails()
    {
        var productId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var items = new[]
        {
            NewItem(productId: productId),
            NewItem(productId: productId)
        };

        var exception = Assert.Throws<DomainException>(() =>
            Order.Create(NewOrderId(), NewCustomerId(), items, Now));

        Assert.Equal("An order cannot contain duplicate products.", exception.Message);
    }

    [Fact]
    public void MarkShipmentDelivered_AfterValidFlow_CompletesOrder()
    {
        var order = CreateOrder();

        order.MarkStockReserved(Now);
        order.BeginPayment(Now);
        order.MarkPaymentCompleted(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Now);
        order.MarkShipmentCreated(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Now);
        order.MarkShipmentDelivered(Now);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(Now, order.CompletedAt);
        Assert.Contains(order.DomainEvents, domainEvent => domainEvent is OrderCompleted);
    }

    [Fact]
    public void BeginPayment_BeforeStockReserved_Fails()
    {
        var order = CreateOrder();

        var exception = Assert.Throws<DomainException>(() => order.BeginPayment(Now));

        Assert.Equal("Payment can start only after stock is reserved.", exception.Message);
    }

    [Fact]
    public void MarkStockRejected_WhenCreated_FailsOrder()
    {
        var order = CreateOrder();

        order.MarkStockRejected(new FailureReason("insufficient stock"), Now);

        Assert.Equal(OrderStatus.Failed, order.Status);
        Assert.Equal(Now, order.FailedAt);
        Assert.Contains(order.DomainEvents, domainEvent => domainEvent is OrderFailed);
    }

    [Fact]
    public void MarkPaymentFailed_WhenPaymentPending_FailsOrder()
    {
        var order = CreateOrder();
        order.MarkStockReserved(Now);
        order.BeginPayment(Now);

        order.MarkPaymentFailed(new FailureReason("payment declined"), Now);

        Assert.Equal(OrderStatus.Failed, order.Status);
        Assert.Equal(Now, order.FailedAt);
        Assert.Contains(order.DomainEvents, domainEvent => domainEvent is OrderFailed);
    }

    [Fact]
    public void Cancel_AfterPaymentStarted_Fails()
    {
        var order = CreateOrder();
        order.MarkStockReserved(Now);
        order.BeginPayment(Now);

        var exception = Assert.Throws<DomainException>(() =>
            order.Cancel(new CancellationReason("customer request"), Now));

        Assert.Equal("Only created or stock-reserved orders can be cancelled.", exception.Message);
    }

    [Fact]
    public void Fail_AfterCompletion_Fails()
    {
        var order = CreateOrder();
        order.MarkStockReserved(Now);
        order.BeginPayment(Now);
        order.MarkPaymentCompleted(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Now);
        order.MarkShipmentCreated(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), Now);
        order.MarkShipmentDelivered(Now);

        var exception = Assert.Throws<DomainException>(() =>
            order.Fail(new FailureReason("late failure"), Now));

        Assert.Equal("Terminal orders cannot be failed.", exception.Message);
    }

    private static Order CreateOrder() =>
        Order.Create(NewOrderId(), NewCustomerId(), [NewItem()], Now);

    private static OrderItem NewItem(Guid? productId = null, string currency = "EUR") =>
        new(
            Guid.NewGuid(),
            new ProductId(productId ?? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            new Sku("kb-rgb-it"),
            "Keyboard",
            2,
            new Money(10m, currency));

    private static OrderId NewOrderId() => new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    private static CustomerId NewCustomerId() => new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
}
