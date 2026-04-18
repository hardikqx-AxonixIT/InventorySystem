using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace InventorySystem.WebAPI.Tests;

public class ApiIntegrationTests : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(InventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_endpoint_is_public()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Transactions_bootstrap_without_token_returns_401()
    {
        var response = await _client.GetAsync("/api/Transactions/bootstrap");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_returns_token_and_bootstrap_succeeds()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            Email = "admin@axonix.local",
            Password = "Admin@123"
        });

        loginResponse.EnsureSuccessStatusCode();
        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(loginJson.TryGetProperty("token", out var tokenProp));
        var token = tokenProp.GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var boot = await _client.GetAsync("/api/Transactions/bootstrap");
        boot.EnsureSuccessStatusCode();
        var body = await boot.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("purchaseOrders", out _));
    }

    [Fact]
    public async Task Stock_in_increases_on_hand()
    {
        await LoginAsync();

        var boot = await _client.GetFromJsonAsync<JsonElement>("/api/Transactions/bootstrap");
        var firstLevel = boot.GetProperty("stockLevels")[0];
        var productId = firstLevel.GetProperty("productId").GetInt32();
        var binId = firstLevel.GetProperty("binId").GetInt32();

        var stockBefore = FindStockQty(boot, productId, binId);

        var movement = await _client.PostAsJsonAsync("/api/Transactions/inventory/stock-in", new
        {
            productId,
            binId,
            quantity = 3m,
            referenceNo = "TEST-STK-IN"
        });
        movement.EnsureSuccessStatusCode();

        var bootAfter = await _client.GetFromJsonAsync<JsonElement>("/api/Transactions/bootstrap");
        var stockAfter = FindStockQty(bootAfter, productId, binId);
        Assert.Equal(stockBefore + 3m, stockAfter);
    }

    private async Task LoginAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            Email = "admin@axonix.local",
            Password = "Admin@123"
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginJson = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = loginJson.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static decimal FindStockQty(JsonElement boot, int productId, int binId)
    {
        foreach (var row in boot.GetProperty("stockLevels").EnumerateArray())
        {
            if (row.GetProperty("productId").GetInt32() == productId &&
                row.GetProperty("binId").GetInt32() == binId)
            {
                return row.GetProperty("quantityOnHand").GetDecimal();
            }
        }

        return 0;
    }
}
