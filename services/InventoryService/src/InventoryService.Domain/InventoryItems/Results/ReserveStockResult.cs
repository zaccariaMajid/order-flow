namespace InventoryService.Domain.Inventory;

public sealed record ReserveStockResult(
    ReservationOutcome Outcome,
    StockReservation? Reservation,
    int RequestedQuantity,
    int AvailableQuantity)
{
    public static ReserveStockResult Reserved(StockReservation reservation) =>
        new(ReservationOutcome.Reserved, reservation, reservation.Quantity, 0);

    public static ReserveStockResult AlreadyReserved(StockReservation reservation) =>
        new(ReservationOutcome.AlreadyReserved, reservation, reservation.Quantity, 0);

    public static ReserveStockResult Rejected(int requestedQuantity, int availableQuantity) =>
        new(ReservationOutcome.Rejected, null, requestedQuantity, availableQuantity);
}
