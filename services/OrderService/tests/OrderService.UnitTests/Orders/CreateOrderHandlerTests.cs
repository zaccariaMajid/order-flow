using OrderService.Application.Orders.Create;
using OrderService.Application.Orders.Persistence;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Domain.ValueObjects;

namespace OrderService.UnitTests.Orders;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_creates_order_with_eur_prices_and_commits()
    {
        RecordingOrderRepository repository = new();
        RecordingUnitOfWork unitOfWork = new();
        CreateOrderHandler handler = new(repository, unitOfWork);
        CreateOrderCommand command = new(
            Guid.Parse("7d6b0912-3f65-48e2-a20f-3f950bd7d7c3"),
            [
                new CreateOrderItem(
                    Guid.Parse("b77d1a1e-8a65-42dd-839a-73f408cc2960"),
                    "Mechanical Keyboard",
                    2,
                    99.99m)
            ]);

        CreateOrderResult result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.OrderId);
        Assert.NotNull(repository.AddedOrder);
        Assert.Equal(result.OrderId, repository.AddedOrder.Id.Value);
        Assert.Equal(199.98m, repository.AddedOrder.TotalAmount.Amount);
        Assert.Equal(Currency.EUR, repository.AddedOrder.TotalAmount.Currency);
        Assert.Equal(1, unitOfWork.SaveChangesCalls);
    }

    private sealed class RecordingOrderRepository : IOrderRepository
    {
        public Order? AddedOrder { get; private set; }

        public Task<Order?> GetByIdAsync(OrderId orderId, CancellationToken cancellationToken) =>
            Task.FromResult<Order?>(null);

        public Task AddAsync(Order order, CancellationToken cancellationToken)
        {
            AddedOrder = order;

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class RecordingUnitOfWork : IOrderUnitOfWork
    {
        public int SaveChangesCalls { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalls++;

            return Task.CompletedTask;
        }
    }
}
