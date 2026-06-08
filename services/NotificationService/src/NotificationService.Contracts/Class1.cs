namespace NotificationService.Contracts;

public static class NotificationServiceContracts
{
    public const string ServiceName = "NotificationService";

    public static readonly string[] ConsumedEvents =
    [
        "OrderCompleted",
        "OrderFailed",
        "ShipmentCreated",
        "ShipmentDelivered"
    ];
}
