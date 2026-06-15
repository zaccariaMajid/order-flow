namespace OrderService.Application.Orders.Persistence;

public interface IOrderUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
