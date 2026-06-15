namespace OrderService.Application.Orders.GetById;

public sealed record OrderDetailsDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    IReadOnlyCollection<OrderItemDetailsDto> Items);

public sealed record OrderItemDetailsDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
