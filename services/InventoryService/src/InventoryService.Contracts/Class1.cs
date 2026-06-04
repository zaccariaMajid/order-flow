namespace InventoryService.Contracts;

public static class InventoryServiceContracts
{
    public const string ServiceName = "InventoryService";

    public static readonly string[] PublishedEvents =
    [
        "StockReserved",
        "StockRejected"
    ];
}
