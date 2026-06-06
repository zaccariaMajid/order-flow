namespace InventoryService.Domain.Inventory;

public sealed class InventoryItem
{
    private readonly List<StockReservation> _reservations = [];
    private readonly List<IDomainEvent> _domainEvents = [];
    private bool _lowStockThresholdReached;

    private InventoryItem(ProductId productId, Sku sku, StockQuantity physicalStock, StockQuantity reorderThreshold)
    {
        ProductId = productId;
        Sku = sku;
        PhysicalStock = physicalStock.Value;
        ReorderThreshold = reorderThreshold.Value;
    }

    public ProductId ProductId { get; }

    public Sku Sku { get; }

    public int PhysicalStock { get; private set; }

    public int ReservedStock => _reservations
        .Where(reservation => reservation.Status == ReservationStatus.Reserved)
        .Sum(reservation => reservation.Quantity);

    public int AvailableStock => PhysicalStock - ReservedStock;

    public int ReorderThreshold { get; private set; }

    public long Version { get; private set; }

    public IReadOnlyCollection<StockReservation> Reservations => _reservations.AsReadOnly();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static InventoryItem Create(ProductId productId, Sku sku, int physicalStock, int reorderThreshold = 0) =>
        Create(
            productId,
            sku,
            StockQuantity.FromStockLevel(physicalStock, "Physical stock"),
            StockQuantity.FromStockLevel(reorderThreshold, "Reorder threshold"));

    public static InventoryItem Create(
        ProductId productId,
        Sku sku,
        StockQuantity physicalStock,
        StockQuantity reorderThreshold)
    {
        var item = new InventoryItem(productId, sku, physicalStock, reorderThreshold);
        item.RecordLowStockEventIfNeeded(DateTimeOffset.UtcNow);
        return item;
    }

    public ReserveStockResult ReserveStock(
        ReservationId reservationId,
        OrderId orderId,
        int quantity,
        DateTimeOffset occurredAt) =>
        ReserveStock(reservationId, orderId, StockQuantity.FromReservation(quantity), occurredAt);

    public ReserveStockResult ReserveStock(
        ReservationId reservationId,
        OrderId orderId,
        StockQuantity quantity,
        DateTimeOffset occurredAt)
    {
        var existingReservation = _reservations.SingleOrDefault(reservation =>
            reservation.OrderId == orderId && reservation.Status == ReservationStatus.Reserved);

        if (existingReservation is not null)
        {
            return ReserveStockResult.AlreadyReserved(existingReservation);
        }

        if (AvailableStock < quantity.Value)
        {
            Record(new StockRejected(orderId, ProductId, Sku, quantity.Value, AvailableStock, occurredAt));
            return ReserveStockResult.Rejected(quantity.Value, AvailableStock);
        }

        var stockReservation = StockReservation.Reserve(reservationId, orderId, quantity, occurredAt);
        _reservations.Add(stockReservation);
        Version++;

        Record(new StockReserved(reservationId, orderId, ProductId, Sku, quantity.Value, occurredAt));
        RecordLowStockEventIfNeeded(occurredAt);

        return ReserveStockResult.Reserved(stockReservation);
    }

    public void ReleaseStock(ReservationId reservationId, DateTimeOffset releasedAt)
    {
        var reservation = _reservations.SingleOrDefault(item => item.Id == reservationId);
        if (reservation is null)
        {
            throw new DomainException("Reservation was not found.");
        }

        reservation.Release(releasedAt);
        Version++;

        Record(new StockReleased(reservation.Id, reservation.OrderId, ProductId, Sku, reservation.Quantity, releasedAt));
        RecordLowStockEventIfNeeded(releasedAt);
    }

    public void ConfirmStockConsumed(ReservationId reservationId, DateTimeOffset consumedAt)
    {
        var reservation = _reservations.SingleOrDefault(item => item.Id == reservationId);
        if (reservation is null)
        {
            throw new DomainException("Reservation was not found.");
        }

        reservation.Consume(consumedAt);
        PhysicalStock -= reservation.Quantity;
        Version++;

        Record(new StockConsumed(reservation.Id, reservation.OrderId, ProductId, Sku, reservation.Quantity, consumedAt));
        RecordLowStockEventIfNeeded(consumedAt);
    }

    public void AdjustPhysicalStock(int newPhysicalStock, string reason, DateTimeOffset occurredAt) =>
        AdjustPhysicalStock(
            StockQuantity.FromStockLevel(newPhysicalStock, "Physical stock"),
            new AdjustmentReason(reason),
            occurredAt);

    public void AdjustPhysicalStock(StockQuantity newPhysicalStock, AdjustmentReason reason, DateTimeOffset occurredAt)
    {
        if (newPhysicalStock.Value < ReservedStock)
        {
            throw new DomainException("Physical stock cannot be less than reserved stock.");
        }

        var previousQuantity = PhysicalStock;
        PhysicalStock = newPhysicalStock.Value;
        Version++;

        Record(new PhysicalStockAdjusted(ProductId, Sku, previousQuantity, newPhysicalStock.Value, reason.Value, occurredAt));
        RecordLowStockEventIfNeeded(occurredAt);
    }

    public void SetReorderThreshold(int reorderThreshold) =>
        SetReorderThreshold(StockQuantity.FromStockLevel(reorderThreshold, "Reorder threshold"), DateTimeOffset.UtcNow);

    public void SetReorderThreshold(StockQuantity reorderThreshold, DateTimeOffset occurredAt)
    {
        ReorderThreshold = reorderThreshold.Value;
        Version++;
        RecordLowStockEventIfNeeded(occurredAt);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecordLowStockEventIfNeeded(DateTimeOffset occurredAt)
    {
        var isLowStock = AvailableStock <= ReorderThreshold;
        if (isLowStock && !_lowStockThresholdReached)
        {
            Record(new LowStockThresholdReached(ProductId, Sku, AvailableStock, ReorderThreshold, occurredAt));
        }

        _lowStockThresholdReached = isLowStock;
    }

    private void Record(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
