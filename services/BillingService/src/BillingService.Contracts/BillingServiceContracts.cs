namespace BillingService.Contracts;

public static class BillingServiceContracts
{
    public const string ServiceName = "BillingService";

    public static readonly string[] PublishedEvents =
    [
        "PaymentCompleted",
        "PaymentFailed"
    ];
}
