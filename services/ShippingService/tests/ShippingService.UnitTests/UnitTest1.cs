using ShippingService.Contracts;
using ShippingService.Domain;

namespace ShippingService.UnitTests;

public class ServiceMetadataTests
{
    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(ShippingServiceDomain.Name, ShippingServiceContracts.ServiceName);
        Assert.Contains("ShipmentCreated", ShippingServiceContracts.PublishedEvents);
    }
}
