namespace OrderService.Application.Orders.Create;

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyCollection<CreateOrderItem> Items);

public sealed record CreateOrderItem(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
