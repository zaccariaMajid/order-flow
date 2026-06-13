using BuildingBlocks.Api;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var serviceName = "OrderService";

builder.Services.AddOpenApi();
builder.Services.AddOrderInfrastructure(builder.Configuration);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => new ServiceInfo(serviceName, "Running"))
    .WithName("GetServiceInfo");

app.MapGet("/health", () => Results.Ok(new { Status = "This is a healthy service testing automatic deploy" }))
    .WithName("GetHealth");

app.MapGet("/health/ready", () => Results.Ok(new { Status = "Healthyyyy" }))
    .WithName("GetReadiness");

app.MapGet("/health/db", async (OrderDbContext dbContext, CancellationToken cancellationToken) =>
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            DateTimeOffset checkedAt = DateTimeOffset.UtcNow;

            if (canConnect)
            {
                return Results.Ok(new
                {
                    Service = serviceName,
                    Dependency = "OrderDb",
                    Status = "Healthy",
                    CheckedAt = checkedAt
                });
            }

            return Results.Json(
                new
                {
                    Service = serviceName,
                    Dependency = "OrderDb",
                    Status = "Unhealthy",
                    CheckedAt = checkedAt,
                    Message = "Database connection failed."
                },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return Results.Json(
                new
                {
                    Service = serviceName,
                    Dependency = "OrderDb",
                    Status = "Unhealthy",
                    CheckedAt = DateTimeOffset.UtcNow,
                    Message = "Database connection failed."
                },
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    })
    .WithName("GetDatabaseHealth");

app.Run();
