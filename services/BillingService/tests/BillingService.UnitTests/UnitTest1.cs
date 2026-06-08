using BillingService.Contracts;
using BillingService.Domain;

namespace BillingService.UnitTests;

public class ServiceMetadataTests
{
    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(BillingServiceDomain.Name, BillingServiceContracts.ServiceName);
        Assert.Contains("PaymentCompleted", BillingServiceContracts.PublishedEvents);
    }
}
