namespace OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    StockReserved = 2,
    Paid = 3,
    ShippingInProgress = 4,
    Completed = 5,
    Cancelled = 6,
    Failed = 7
}
