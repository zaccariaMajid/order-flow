using NotificationService.Contracts;
using NotificationService.Domain;

namespace NotificationService.UnitTests;

public class ServiceMetadataTests
{
    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(NotificationServiceDomain.Name, NotificationServiceContracts.ServiceName);
        Assert.Contains("OrderCompleted", NotificationServiceContracts.ConsumedEvents);
    }
}
