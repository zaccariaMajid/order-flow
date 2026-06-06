namespace InventoryService.Domain.Inventory;

public sealed record PhysicalStockAdjusted(
    ProductId ProductId,
    Sku Sku,
    int PreviousQuantity,
    int NewQuantity,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
