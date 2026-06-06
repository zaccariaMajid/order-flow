namespace OrderService.Domain.Orders;

public sealed class OrderItem
{
    public OrderItem(
        Guid id,
        ProductId productId,
        Sku sku,
        string productName,
        int quantity,
        Money unitPrice)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Order item identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Order item quantity must be greater than zero.");
        }

        Id = id;
        ProductId = productId;
        Sku = sku;
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
        LineTotal = unitPrice.Multiply(quantity);
    }

    public Guid Id { get; }

    public ProductId ProductId { get; }

    public Sku Sku { get; }

    public string ProductName { get; }

    public int Quantity { get; }

    public Money UnitPrice { get; }

    public Money LineTotal { get; }
}
