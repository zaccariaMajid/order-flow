namespace InventoryService.Domain.Inventory;

public sealed record ReservationId
{
    public ReservationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("Reservation identifier is required.");
        }

        Value = value;
    }

    public Guid Value { get; }
}
