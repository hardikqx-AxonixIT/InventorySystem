using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace InventorySystem.WebAPI.Tests;

public class InventoryApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Testing:UseInMemoryDatabase", "true");
        builder.UseSetting("Testing:InMemoryDatabaseName", $"test-{Guid.NewGuid():N}");
        builder.UseSetting("Security:RequestsPerMinutePerIp", "100000");
        builder.UseSetting("Commercial:EnforceSubscription", "false");
    }
}
