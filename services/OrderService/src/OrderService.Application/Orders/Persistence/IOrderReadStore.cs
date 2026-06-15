using OrderService.Application.Orders.GetById;

namespace OrderService.Application.Orders.Persistence;

public interface IOrderReadStore
{
    Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);
}
