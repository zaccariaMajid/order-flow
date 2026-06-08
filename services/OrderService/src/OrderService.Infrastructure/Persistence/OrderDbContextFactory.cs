using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderService.Infrastructure.Persistence;

public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    private const string LocalConnectionString = "Host=localhost;Database=orderflow;Username=orderflow;Password=orderflow";

    public OrderDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__OrderDb")
            ?? LocalConnectionString;

        DbContextOptionsBuilder<OrderDbContext> optionsBuilder = new();
        optionsBuilder.UseNpgsql(ConnectionStringNormalizer.NormalizePostgresConnectionString(connectionString));

        return new OrderDbContext(optionsBuilder.Options);
    }
}
