using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id");

        builder.Property<OrderId>("OrderId")
            .HasConversion(
                orderId => orderId.Value,
                value => new OrderId(value))
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(item => item.ProductId)
            .HasConversion(
                productId => productId.Value,
                value => new ProductId(value))
            .HasColumnName("product_id")
            .IsRequired();

        builder.Property(item => item.ProductName)
            .HasColumnName("product_name")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.OwnsOne(
            item => item.UnitPrice,
            moneyBuilder =>
            {
                moneyBuilder.Property(money => money.Amount)
                    .HasColumnName("unit_price_amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(money => money.Currency)
                    .HasConversion<string>()
                    .HasColumnName("unit_price_currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        builder.Ignore(item => item.TotalPrice);

        builder.HasIndex("OrderId");

        builder.HasIndex("OrderId", nameof(OrderItem.ProductId))
            .IsUnique();
    }
}
