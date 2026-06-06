namespace InventoryService.Domain.Inventory;

public sealed record StockRejected(
    OrderId OrderId,
    ProductId ProductId,
    Sku Sku,
    int RequestedQuantity,
    int AvailableQuantity,
    DateTimeOffset OccurredAt) : IDomainEvent;
