namespace InventoryService.Domain.Inventory;

public sealed record StockReleased(
    ReservationId ReservationId,
    OrderId OrderId,
    ProductId ProductId,
    Sku Sku,
    int Quantity,
    DateTimeOffset OccurredAt) : IDomainEvent;
