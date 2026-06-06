namespace InventoryService.Domain.Inventory;

public sealed record LowStockThresholdReached(
    ProductId ProductId,
    Sku Sku,
    int AvailableQuantity,
    int Threshold,
    DateTimeOffset OccurredAt) : IDomainEvent;
