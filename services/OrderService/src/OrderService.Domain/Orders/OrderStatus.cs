namespace OrderService.Domain.Orders;

public enum OrderStatus
{
    Created = 0,
    StockReserved = 1,
    PaymentPending = 2,
    Paid = 3,
    ShippingInProgress = 4,
    Completed = 5,
    Cancelled = 6,
    Failed = 7
}
