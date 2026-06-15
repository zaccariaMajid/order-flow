using BuildingBlocks.Api;
using System.Diagnostics;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Orders.Create;
using OrderService.Application.Orders.GetById;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var serviceName = "OrderService";

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();
builder.Services.AddScoped<CreateOrderHandler>();
builder.Services.AddScoped<GetOrderByIdHandler>();
builder.Services.AddOrderInfrastructure(builder.Configuration);
var app = builder.Build();
var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
var startupLogger = loggerFactory.CreateLogger("OrderService.Startup");
var requestLogger = loggerFactory.CreateLogger("OrderService.Requests");
var healthLogger = loggerFactory.CreateLogger("OrderService.Health");

startupLogger.LogInformation("{ServiceName} starting", serviceName);
startupLogger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
startupLogger.LogInformation(
    "Database configured: {Configured}",
    !string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("OrderDb")));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    Stopwatch stopwatch = Stopwatch.StartNew();
    string method = context.Request.Method;
    PathString path = context.Request.Path;

    requestLogger.LogInformation("HTTP request started {Method} {Path}", method, path);

    try
    {
        await next(context);
        stopwatch.Stop();

        requestLogger.LogInformation(
            "HTTP request completed {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
            method,
            path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
    catch (Exception exception)
    {
        stopwatch.Stop();

        requestLogger.LogError(
            exception,
            "HTTP request failed {Method} {Path} after {ElapsedMilliseconds}ms",
            method,
            path,
            stopwatch.ElapsedMilliseconds);

        throw;
    }
});

app.MapGet("/", () => new ServiceInfo(serviceName, "Running"))
    .WithName("GetServiceInfo");

app.MapGet("/health", () =>
    {
        healthLogger.LogInformation("Health check executed");

        return Results.Ok(new { Status = "This is a healthy service testing automatic deploy" });
    })
    .WithName("GetHealth");

app.MapGet("/health/ready", () =>
    {
        healthLogger.LogInformation("Readiness check executed");

        return Results.Ok(new { Status = "Healthyyyy" });
    })
    .WithName("GetReadiness");

app.MapGet("/health/db", async (OrderDbContext dbContext, CancellationToken cancellationToken) =>
    {
        try
        {
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            DateTimeOffset checkedAt = DateTimeOffset.UtcNow;

            healthLogger.LogInformation("Database connectivity check: {CanConnect}", canConnect);

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

            healthLogger.LogWarning("Database connectivity check failed");

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
            healthLogger.LogError(exception, "Database readiness check failed");

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

app.MapPost("/orders", async (
        CreateOrderRequest request,
        IValidator<CreateOrderCommand> validator,
        CreateOrderHandler handler,
        CancellationToken cancellationToken) =>
    {
        CreateOrderCommand command = request.ToCommand();
        FluentValidation.Results.ValidationResult validationResult =
            await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        CreateOrderResult result = await handler.HandleAsync(command, cancellationToken);

        return Results.Created($"/orders/{result.OrderId}", new CreateOrderResponse(result.OrderId));
    })
    .WithName("CreateOrder");

app.MapGet("/orders/{id}", async (
        string id,
        GetOrderByIdHandler handler,
        CancellationToken cancellationToken) =>
    {
        if (!Guid.TryParse(id, out Guid orderId) || orderId == Guid.Empty)
        {
            return Results.BadRequest(new { Error = "Order id must be a valid GUID." });
        }

        OrderDetailsDto? order = await handler.HandleAsync(new GetOrderByIdQuery(orderId), cancellationToken);

        return order is null
            ? Results.NotFound()
            : Results.Ok(order);
    })
    .WithName("GetOrderById");

startupLogger.LogInformation("{ServiceName} started", serviceName);

app.Run();

internal sealed record CreateOrderRequest(Guid CustomerId, IReadOnlyCollection<CreateOrderItemRequest> Items)
{
    public CreateOrderCommand ToCommand() =>
        new(
            CustomerId,
            (Items ?? []).Select(item => new CreateOrderItem(
                    item.ProductId,
                    item.ProductName ?? string.Empty,
                    item.Quantity,
                    item.UnitPrice))
                .ToArray());
}

internal sealed record CreateOrderItemRequest(Guid ProductId, string? ProductName, int Quantity, decimal UnitPrice);

internal sealed record CreateOrderResponse(Guid OrderId);
