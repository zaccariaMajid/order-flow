namespace OrderService.Contracts;

public static class OrderServiceContracts
{
    public const string ServiceName = "OrderService";

    public static readonly string[] PublishedEvents =
    [
        "OrderCreated",
        "OrderCancelled",
        "OrderCompleted",
        "OrderFailed"
    ];
}
