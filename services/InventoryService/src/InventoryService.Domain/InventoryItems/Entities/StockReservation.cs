namespace InventoryService.Domain.Inventory;

public sealed class StockReservation
{
    private StockReservation(
        ReservationId id,
        OrderId orderId,
        StockQuantity quantity,
        DateTimeOffset createdAt)
    {
        Id = id;
        OrderId = orderId;
        Quantity = quantity.Value;
        Status = ReservationStatus.Reserved;
        CreatedAt = createdAt;
    }

    public ReservationId Id { get; }

    public OrderId OrderId { get; }

    public int Quantity { get; }

    public ReservationStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ReleasedAt { get; private set; }

    public DateTimeOffset? ConsumedAt { get; private set; }

    public static StockReservation Reserve(
        ReservationId id,
        OrderId orderId,
        int quantity,
        DateTimeOffset createdAt) =>
        Reserve(id, orderId, StockQuantity.FromReservation(quantity), createdAt);

    public static StockReservation Reserve(
        ReservationId id,
        OrderId orderId,
        StockQuantity quantity,
        DateTimeOffset createdAt) =>
        new(id, orderId, quantity, createdAt);

    public void Release(DateTimeOffset releasedAt)
    {
        if (Status != ReservationStatus.Reserved)
        {
            throw new DomainException("Only reserved stock can be released.");
        }

        Status = ReservationStatus.Released;
        ReleasedAt = releasedAt;
    }

    public void Consume(DateTimeOffset consumedAt)
    {
        if (Status != ReservationStatus.Reserved)
        {
            throw new DomainException("Only reserved stock can be consumed.");
        }

        Status = ReservationStatus.Consumed;
        ConsumedAt = consumedAt;
    }
}
