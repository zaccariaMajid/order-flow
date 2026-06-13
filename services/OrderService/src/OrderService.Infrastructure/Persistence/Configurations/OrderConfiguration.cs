using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasConversion(
                orderId => orderId.Value,
                value => new OrderId(value))
            .HasColumnName("id");

        builder.Property(order => order.CustomerId)
            .HasConversion(
                customerId => customerId.Value,
                value => new CustomerId(value))
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(
            order => order.TotalAmount,
            moneyBuilder =>
            {
                moneyBuilder.Property(money => money.Amount)
                    .HasColumnName("total_amount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                moneyBuilder.Property(money => money.Currency)
                    .HasConversion<string>()
                    .HasColumnName("currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

        builder.Property(order => order.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(order => order.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(order => order.CancelledAt)
            .HasColumnName("cancelled_at");

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(order => order.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(order => order.DomainEvents);

        builder.HasIndex(order => order.CustomerId);

        builder.HasIndex(order => order.Status);
    }
}
