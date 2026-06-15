using OrderService.Application.Orders.Persistence;

namespace OrderService.Application.Orders.GetById;

public sealed class GetOrderByIdHandler
{
    private readonly IOrderReadStore _orders;

    public GetOrderByIdHandler(IOrderReadStore orders)
    {
        _orders = orders;
    }

    public Task<OrderDetailsDto?> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken) =>
        _orders.GetByIdAsync(query.OrderId, cancellationToken);
}
