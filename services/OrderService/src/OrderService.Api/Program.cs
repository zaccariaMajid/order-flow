using BuildingBlocks.Api;
using OrderService.Infrastructure;

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

app.MapGet("/health", () => Results.Ok(new ServiceInfo(serviceName, "Healthy")))
    .WithName("GetHealth");

app.Run();
