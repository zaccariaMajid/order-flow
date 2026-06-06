namespace ShippingService.Contracts;

public static class ShippingServiceContracts
{
    public const string ServiceName = "ShippingService";

    public static readonly string[] PublishedEvents =
    [
        "ShipmentCreated",
        "ShipmentDelivered"
    ];
}
