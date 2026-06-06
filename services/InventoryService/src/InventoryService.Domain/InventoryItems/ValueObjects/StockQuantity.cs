namespace InventoryService.Domain.Inventory;

public sealed record StockQuantity
{
    private StockQuantity(int value)
    {
        Value = value;
    }

    public int Value { get; }

    public static StockQuantity FromStockLevel(int value, string name)
    {
        if (value < 0)
        {
            throw new DomainException($"{name} cannot be negative.");
        }

        return new StockQuantity(value);
    }

    public static StockQuantity FromReservation(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("Reservation quantity must be greater than zero.");
        }

        return new StockQuantity(value);
    }
}
