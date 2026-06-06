namespace InventoryService.Domain.Inventory;

public sealed record StockReserved(
    ReservationId ReservationId,
    OrderId OrderId,
    ProductId ProductId,
    Sku Sku,
    int Quantity,
    DateTimeOffset OccurredAt) : IDomainEvent;
