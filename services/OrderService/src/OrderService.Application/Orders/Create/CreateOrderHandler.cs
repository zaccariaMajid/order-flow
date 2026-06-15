using OrderService.Application.Orders.Persistence;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Orders.Create;

public sealed class CreateOrderHandler
{
    private readonly IOrderRepository _orders;
    private readonly IOrderUnitOfWork _unitOfWork;

    public CreateOrderHandler(IOrderRepository orders, IOrderUnitOfWork unitOfWork)
    {
        _orders = orders;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOrderResult> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        List<OrderItem> items = command.Items
            .Select(item => OrderItem.Create(
                new ProductId(item.ProductId),
                item.ProductName,
                item.Quantity,
                new Money(item.UnitPrice, Currency.EUR)))
            .ToList();

        Order order = Order.Create(
            new CustomerId(command.CustomerId),
            items,
            DateTimeOffset.UtcNow);

        await _orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id.Value);
    }
}
