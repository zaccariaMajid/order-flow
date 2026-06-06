namespace OrderService.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items;
    private readonly List<IDomainEvent> _domainEvents = [];

    private Order(OrderId id, CustomerId customerId, IReadOnlyCollection<OrderItem> items, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerId = customerId;
        _items = [.. items];
        Status = OrderStatus.Created;
        CreatedAt = createdAt;
        TotalAmount = Money.Sum(_items.Select(item => item.LineTotal));
    }

    public OrderId Id { get; }

    public CustomerId CustomerId { get; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public OrderStatus Status { get; private set; }

    public Money TotalAmount { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public DateTimeOffset? FailedAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Order Create(
        OrderId id,
        CustomerId customerId,
        IEnumerable<OrderItem> items,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(items);

        var orderItems = items.ToArray();
        if (orderItems.Length == 0)
        {
            throw new DomainException("An order must contain at least one item.");
        }

        if (orderItems.Select(item => item.ProductId).Distinct().Count() != orderItems.Length)
        {
            throw new DomainException("An order cannot contain duplicate products.");
        }

        var currency = orderItems[0].UnitPrice.Currency;
        if (orderItems.Any(item => item.UnitPrice.Currency != currency))
        {
            throw new DomainException("All order items must use the same currency.");
        }

        var order = new Order(id, customerId, orderItems, createdAt);
        if (order.TotalAmount.Amount <= 0)
        {
            throw new DomainException("Order total amount must be greater than zero.");
        }

        order.Record(new OrderCreated(order.Id, order.CustomerId, order.TotalAmount, createdAt));
        return order;
    }

    public void MarkStockReserved(DateTimeOffset occurredAt)
    {
        EnsureStatus(OrderStatus.Created, "Only created orders can be marked as stock reserved.");

        Status = OrderStatus.StockReserved;
        Record(new OrderStockReserved(Id, occurredAt));
    }

    public void MarkStockRejected(FailureReason reason, DateTimeOffset occurredAt)
    {
        EnsureStatus(OrderStatus.Created, "Only created orders can be failed after stock rejection.");

        Status = OrderStatus.Failed;
        FailedAt = occurredAt;
        Record(new OrderFailed(Id, reason, occurredAt));
    }

    public void BeginPayment(DateTimeOffset occurredAt)
    {
        EnsureStatus(OrderStatus.StockReserved, "Payment can start only after stock is reserved.");

        Status = OrderStatus.PaymentPending;
        Record(new OrderPaymentStarted(Id, occurredAt));
    }

    public void MarkPaymentCompleted(Guid paymentId, DateTimeOffset occurredAt)
    {
        if (paymentId == Guid.Empty)
        {
            throw new DomainException("Payment identifier is required.");
        }

        EnsureStatus(OrderStatus.PaymentPending, "Only payment-pending orders can be marked as paid.");

        Status = OrderStatus.Paid;
        Record(new OrderPaid(Id, paymentId, occurredAt));
    }

    public void MarkPaymentFailed(FailureReason reason, DateTimeOffset occurredAt)
    {
        EnsureStatus(OrderStatus.PaymentPending, "Only payment-pending orders can fail payment.");

        Status = OrderStatus.Failed;
        FailedAt = occurredAt;
        Record(new OrderFailed(Id, reason, occurredAt));
    }

    public void MarkShipmentCreated(Guid shipmentId, DateTimeOffset occurredAt)
    {
        if (shipmentId == Guid.Empty)
        {
            throw new DomainException("Shipment identifier is required.");
        }

        EnsureStatus(OrderStatus.Paid, "Only paid orders can enter shipping.");

        Status = OrderStatus.ShippingInProgress;
        Record(new OrderShippingStarted(Id, shipmentId, occurredAt));
    }

    public void MarkShipmentDelivered(DateTimeOffset deliveredAt)
    {
        EnsureStatus(OrderStatus.ShippingInProgress, "Only orders in shipping can be completed.");

        Status = OrderStatus.Completed;
        CompletedAt = deliveredAt;
        Record(new OrderCompleted(Id, deliveredAt));
    }

    public void Cancel(CancellationReason reason, DateTimeOffset cancelledAt)
    {
        if (Status is not (OrderStatus.Created or OrderStatus.StockReserved))
        {
            throw new DomainException("Only created or stock-reserved orders can be cancelled.");
        }

        Status = OrderStatus.Cancelled;
        CancelledAt = cancelledAt;
        Record(new OrderCancelled(Id, reason, cancelledAt));
    }

    public void Fail(FailureReason reason, DateTimeOffset failedAt)
    {
        if (IsTerminal())
        {
            throw new DomainException("Terminal orders cannot be failed.");
        }

        Status = OrderStatus.Failed;
        FailedAt = failedAt;
        Record(new OrderFailed(Id, reason, failedAt));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private bool IsTerminal() => Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Failed;

    private void EnsureStatus(OrderStatus expectedStatus, string message)
    {
        if (Status != expectedStatus)
        {
            throw new DomainException(message);
        }
    }

    private void Record(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
