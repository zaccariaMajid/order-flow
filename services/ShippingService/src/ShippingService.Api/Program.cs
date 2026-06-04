using BuildingBlocks.Api;

var builder = WebApplication.CreateBuilder(args);
var serviceName = "ShippingService";

builder.Services.AddOpenApi();
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
