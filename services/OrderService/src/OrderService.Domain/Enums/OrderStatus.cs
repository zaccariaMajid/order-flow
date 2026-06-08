namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    StockReserved = 2,
    PaymentPending = 3,
    Paid = 4,
    ShippingInProgress = 5,
    Completed = 6,
    Cancelled = 7,
    Failed = 8
}
