using BuildingBlocks.Domain;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Events;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates;

public sealed class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly List<OrderItem> _items;

    private Order()
    {
        _items = [];
        CustomerId = default;
        TotalAmount = null!;
    }

    private Order(Guid id, CustomerId customerId, List<OrderItem> items, Money totalAmount, DateTimeOffset createdAt)
    {
        Id = id;
        CustomerId = customerId;
        _items = items;
        TotalAmount = totalAmount;
        CreatedAt = createdAt;
        Status = OrderStatus.Pending;
    }

    public Guid Id { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public OrderStatus Status { get; private set; }

    public Money TotalAmount { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? CancelledAt { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Order Create(CustomerId customerId, IEnumerable<OrderItem> items, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(items);

        List<OrderItem> orderItems = items.ToList();
        if (orderItems.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one item.", nameof(items));
        }

        Money totalAmount = CalculateTotal(orderItems);
        if (totalAmount.Amount <= 0)
        {
            throw new InvalidOperationException("Total amount must be greater than zero.");
        }

        Order order = new(Guid.NewGuid(), customerId, orderItems, totalAmount, createdAt);
        order.AddDomainEvent(new OrderCreated(order.Id, order.CustomerId, order.Items, order.TotalAmount, createdAt));

        return order;
    }

    public void ReserveStock()
    {
        EnsureTransitionFrom(OrderStatus.Pending);

        Status = OrderStatus.StockReserved;
    }

    public void MarkPaymentPending()
    {
        EnsureTransitionFrom(OrderStatus.StockReserved);

        Status = OrderStatus.PaymentPending;
    }

    public void MarkAsPaid()
    {
        EnsureTransitionFrom(OrderStatus.PaymentPending);

        Status = OrderStatus.Paid;
    }

    public void MarkShippingInProgress()
    {
        EnsureTransitionFrom(OrderStatus.Paid);

        Status = OrderStatus.ShippingInProgress;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        EnsureTransitionFrom(OrderStatus.ShippingInProgress);

        Status = OrderStatus.Completed;
        CompletedAt = completedAt;
        AddDomainEvent(new OrderCompleted(Id, completedAt));
    }

    public void Cancel(string reason, DateTimeOffset cancelledAt)
    {
        EnsureReason(reason);
        EnsureTransitionFrom(OrderStatus.Pending);

        Status = OrderStatus.Cancelled;
        CancelledAt = cancelledAt;
        AddDomainEvent(new OrderCancelled(Id, reason.Trim(), cancelledAt));
    }

    public void Fail(string reason, DateTimeOffset failedAt)
    {
        EnsureReason(reason);
        EnsureNotTerminal();

        Status = OrderStatus.Failed;
        AddDomainEvent(new OrderFailed(Id, reason.Trim(), failedAt));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private static Money CalculateTotal(IReadOnlyCollection<OrderItem> items)
    {
        Money total = Money.Zero(items.First().UnitPrice.Currency);

        foreach (OrderItem item in items)
        {
            total = total.Add(item.TotalPrice);
        }

        return total;
    }

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private void EnsureTransitionFrom(OrderStatus expectedStatus)
    {
        EnsureNotTerminal();

        if (Status != expectedStatus)
        {
            throw new InvalidOperationException($"Order cannot transition from {Status} when {expectedStatus} is required.");
        }
    }

    private void EnsureNotTerminal()
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Failed)
        {
            throw new InvalidOperationException($"Order in {Status} status cannot be changed.");
        }
    }

    private static void EnsureReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason cannot be empty.", nameof(reason));
        }
    }
}
