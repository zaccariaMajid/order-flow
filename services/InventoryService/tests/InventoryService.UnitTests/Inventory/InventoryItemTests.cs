using BuildingBlocks.Domain;
using InventoryService.Domain.Inventory;

namespace InventoryService.UnitTests.Inventory;

public sealed class InventoryItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 01, 01, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReserveStock_WhenAvailable_ReservesStockAndRecordsEvent()
    {
        var item = CreateItem(physicalStock: 10);

        var result = item.ReserveStock(NewReservationId(), NewOrderId(), StockQuantity.FromReservation(3), Now);

        Assert.Equal(ReservationOutcome.Reserved, result.Outcome);
        Assert.Equal(3, item.ReservedStock);
        Assert.Equal(7, item.AvailableStock);
        Assert.Contains(item.DomainEvents, domainEvent => domainEvent is StockReserved);
    }

    [Fact]
    public void ReserveStock_WhenInsufficientStock_RejectsAndDoesNotReserve()
    {
        var item = CreateItem(physicalStock: 2);

        var result = item.ReserveStock(NewReservationId(), NewOrderId(), quantity: 3, Now);

        Assert.Equal(ReservationOutcome.Rejected, result.Outcome);
        Assert.Equal(0, item.ReservedStock);
        Assert.Equal(2, item.AvailableStock);
        Assert.Contains(item.DomainEvents, domainEvent => domainEvent is StockRejected);
    }

    [Fact]
    public void ReserveStock_WithDuplicateOrder_ReturnsExistingReservation()
    {
        var item = CreateItem(physicalStock: 10);
        var orderId = NewOrderId();

        var first = item.ReserveStock(NewReservationId(), orderId, quantity: 3, Now);
        var second = item.ReserveStock(new ReservationId(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb")), orderId, quantity: 3, Now);

        Assert.Equal(ReservationOutcome.Reserved, first.Outcome);
        Assert.Equal(ReservationOutcome.AlreadyReserved, second.Outcome);
        Assert.Single(item.Reservations);
        Assert.Equal(3, item.ReservedStock);
    }

    [Fact]
    public void ReleaseStock_WhenReserved_RestoresAvailableStock()
    {
        var item = CreateItem(physicalStock: 10);
        var reservationId = NewReservationId();
        item.ReserveStock(reservationId, NewOrderId(), quantity: 3, Now);

        item.ReleaseStock(reservationId, Now);

        Assert.Equal(0, item.ReservedStock);
        Assert.Equal(10, item.AvailableStock);
        Assert.Contains(item.DomainEvents, domainEvent => domainEvent is StockReleased);
    }

    [Fact]
    public void ConfirmStockConsumed_WhenReserved_ReducesPhysicalStockAndRecordsEvent()
    {
        var item = CreateItem(physicalStock: 10);
        var reservationId = NewReservationId();
        item.ReserveStock(reservationId, NewOrderId(), StockQuantity.FromReservation(3), Now);

        item.ConfirmStockConsumed(reservationId, Now);

        Assert.Equal(7, item.PhysicalStock);
        Assert.Equal(0, item.ReservedStock);
        Assert.Equal(7, item.AvailableStock);
        Assert.Contains(item.DomainEvents, domainEvent => domainEvent is StockConsumed);
    }

    [Fact]
    public void ReleaseStock_WhenConsumed_Fails()
    {
        var item = CreateItem(physicalStock: 10);
        var reservationId = NewReservationId();
        item.ReserveStock(reservationId, NewOrderId(), StockQuantity.FromReservation(3), Now);
        item.ConfirmStockConsumed(reservationId, Now);

        var exception = Assert.Throws<DomainException>(() => item.ReleaseStock(reservationId, Now));

        Assert.Equal("Only reserved stock can be released.", exception.Message);
    }

    [Fact]
    public void ReleaseStock_WhenAlreadyReleased_Fails()
    {
        var item = CreateItem(physicalStock: 10);
        var reservationId = NewReservationId();
        item.ReserveStock(reservationId, NewOrderId(), quantity: 3, Now);
        item.ReleaseStock(reservationId, Now);

        var exception = Assert.Throws<DomainException>(() => item.ReleaseStock(reservationId, Now));

        Assert.Equal("Only reserved stock can be released.", exception.Message);
    }

    [Fact]
    public void AdjustPhysicalStock_BelowReservedStock_Fails()
    {
        var item = CreateItem(physicalStock: 10);
        item.ReserveStock(NewReservationId(), NewOrderId(), quantity: 5, Now);

        var exception = Assert.Throws<DomainException>(() =>
            item.AdjustPhysicalStock(newPhysicalStock: 4, "cycle count", Now));

        Assert.Equal("Physical stock cannot be less than reserved stock.", exception.Message);
    }

    [Fact]
    public void AdjustPhysicalStock_WithEmptyReason_Fails()
    {
        var item = CreateItem(physicalStock: 10);

        var exception = Assert.Throws<DomainException>(() =>
            item.AdjustPhysicalStock(StockQuantity.FromStockLevel(11, "Physical stock"), new AdjustmentReason(" "), Now));

        Assert.Equal("Stock adjustment reason is required.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativePhysicalStock_Fails()
    {
        var exception = Assert.Throws<DomainException>(() => CreateItem(physicalStock: -1));

        Assert.Equal("Physical stock cannot be negative.", exception.Message);
    }

    [Fact]
    public void LowStockThresholdReached_FiresOnlyWhenCrossingThreshold()
    {
        var item = InventoryItem.Create(
            new ProductId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            new Sku("kb-rgb-it"),
            StockQuantity.FromStockLevel(5, "Physical stock"),
            StockQuantity.FromStockLevel(2, "Reorder threshold"));

        item.ReserveStock(NewReservationId(), NewOrderId(), StockQuantity.FromReservation(3), Now);
        item.AdjustPhysicalStock(StockQuantity.FromStockLevel(5, "Physical stock"), new AdjustmentReason("cycle count"), Now);

        Assert.Single(item.DomainEvents.OfType<LowStockThresholdReached>());
    }

    private static InventoryItem CreateItem(int physicalStock) =>
        InventoryItem.Create(
            new ProductId(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa")),
            new Sku("kb-rgb-it"),
            physicalStock,
            reorderThreshold: 1);

    private static ReservationId NewReservationId() => new(Guid.Parse("11111111-1111-1111-1111-111111111111"));

    private static OrderId NewOrderId() => new(Guid.Parse("22222222-2222-2222-2222-222222222222"));
}
