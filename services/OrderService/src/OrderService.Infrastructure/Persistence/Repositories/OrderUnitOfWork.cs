using OrderService.Application.Orders.Persistence;

namespace OrderService.Infrastructure.Persistence.Repositories;

public sealed class OrderUnitOfWork : IOrderUnitOfWork
{
    private readonly OrderDbContext _dbContext;

    public OrderUnitOfWork(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
