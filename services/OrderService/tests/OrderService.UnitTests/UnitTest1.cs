using OrderService.Contracts;
using OrderService.Domain;

namespace OrderService.UnitTests;

public class ServiceMetadataTests
{
    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(OrderServiceDomain.Name, OrderServiceContracts.ServiceName);
        Assert.Contains("OrderCreated", OrderServiceContracts.PublishedEvents);
    }
}
