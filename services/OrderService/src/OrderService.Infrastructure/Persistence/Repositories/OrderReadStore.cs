using Microsoft.EntityFrameworkCore;
using OrderService.Application.Orders.GetById;
using OrderService.Application.Orders.Persistence;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class OrderReadStore : IOrderReadStore
{
    private readonly OrderDbContext _dbContext;

    public OrderReadStore(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderDetailsDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        OrderId id = new(orderId);

        return await _dbContext.Orders
            .AsNoTracking()
            .Where(order => order.Id == id)
            .Select(order => new OrderDetailsDto(
                order.Id.Value,
                order.CustomerId.Value,
                order.Status.ToString(),
                order.TotalAmount.Amount,
                order.TotalAmount.Currency.ToString(),
                order.Items
                    .OrderBy(item => item.ProductName)
                    .Select(item => new OrderItemDetailsDto(
                        item.Id,
                        item.ProductId.Value,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice.Amount,
                        item.UnitPrice.Amount * item.Quantity))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
