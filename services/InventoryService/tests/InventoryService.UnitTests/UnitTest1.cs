using InventoryService.Contracts;
using InventoryService.Domain;

namespace InventoryService.UnitTests;

public class ServiceMetadataTests
{
    [Fact]
    public void Service_metadata_matches_contracts()
    {
        Assert.Equal(InventoryServiceDomain.Name, InventoryServiceContracts.ServiceName);
        Assert.Contains("StockReserved", InventoryServiceContracts.PublishedEvents);
    }
}
