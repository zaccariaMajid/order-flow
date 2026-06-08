using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem(Guid id, ProductId productId, string productName, int quantity, Money unitPrice)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; }

    public ProductId ProductId { get; }

    public string ProductName { get; }

    public int Quantity { get; }

    public Money UnitPrice { get; }

    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    public static OrderItem Create(ProductId productId, string productName, int quantity, Money unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(unitPrice);

        return new OrderItem(Guid.NewGuid(), productId, productName.Trim(), quantity, unitPrice);
    }
}
