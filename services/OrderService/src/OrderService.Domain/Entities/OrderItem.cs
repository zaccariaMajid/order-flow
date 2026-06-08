using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
        ProductId = default;
        ProductName = string.Empty;
        UnitPrice = null!;
    }

    private OrderItem(Guid id, ProductId productId, string productName, int quantity, Money unitPrice)
    {
        Id = id;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid Id { get; private set; }

    public ProductId ProductId { get; private set; }

    public string ProductName { get; private set; }

    public int Quantity { get; private set; }

    public Money UnitPrice { get; private set; }

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
