using System.Net;
using System.Text;
using System.Text.Json;
using Kibo.TestingFramework;

namespace Kibo.LegacyTests;

/// <summary>
/// ⚠️ WARNING — This test class is INTENTIONALLY written with poor practices.
/// It represents a "legacy" test suite that a Lead SDET candidate must refactor.
///
/// Known anti-patterns embedded here:
///   • HttpClient created directly in every test method (no reuse / disposal)
///   • Hardcoded base URL (http://localhost:5000) copy-pasted everywhere
///   • x-kibo-tenant header logic duplicated in every method
///   • Raw JSON strings built inline instead of using a builder/model
///   • Thread.Sleep(6000) used to wait for async status changes (brittle & slow)
/// </summary>
public class OrderTests
{
    [Fact]
    public async Task CreateOrder_ReturnsSuccess()
    {
        using var client = new KiboApiClient();

        var order = new
        {
            CustomerEmail = "john.doe@example.com",
            LineItems = new[]
            {
                new
                {
                    ProductCode = "SKU-001",
                    Quantity = 2,
                    UnitPrice = 29.99m
                }
            }
        };

        var response = await client.PostAsync("/v1/orders", order);

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but received {(int)response.StatusCode} {response.StatusCode}.{Environment.NewLine}{response.Diagnostics}");
        Assert.Contains("Pending", response.Body);
    }

    [Fact]
    public async Task CreateOrder_WithoutTenantHeader_Returns401()
    {
        // BAD: Another brand-new HttpClient
        var client = new HttpClient();

        // BAD: Same hardcoded URL
        var url = "http://localhost:5000/v1/orders";

        // NOTE: Intentionally NOT adding x-kibo-tenant header

        // BAD: Duplicate JSON construction
        var json = """
        {
            "customerEmail": "no-tenant@example.com",
            "lineItems": [
                {
                    "productCode": "SKU-999",
                    "quantity": 1,
                    "unitPrice": 9.99
                }
            ]
        }
        """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOrder_AfterCreation_StatusBecomesReadyForFulfillment()
    {
        // BAD: Yet another HttpClient
        var client = new HttpClient();

        // BAD: Hardcoded URL (again)
        var baseUrl = "http://localhost:5000";

        // BAD: Duplicated header setup
        client.DefaultRequestHeaders.Add("x-kibo-tenant", "tenant-abc-123");

        // BAD: Copy-pasted JSON (third time now)
        var json = """
        {
            "customerEmail": "status-check@example.com",
            "lineItems": [
                {
                    "productCode": "SKU-042",
                    "quantity": 1,
                    "unitPrice": 49.99
                }
            ]
        }
        """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var createResponse = await client.PostAsync($"{baseUrl}/v1/orders", content);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // Extract the order ID from the response
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createdOrder = JsonSerializer.Deserialize<JsonElement>(createBody);
        var orderId = createdOrder.GetProperty("id").GetString();

        // ============================================================
        // 🐛 THE QA FLAW — Thread.Sleep makes this test brittle & slow.
        //    The API transitions the order after 5 seconds, so this
        //    test just sleeps for 6 seconds and hopes for the best.
        //    In CI/CD this will be flaky and waste pipeline time.
        // ============================================================
        Thread.Sleep(6000);

        var getResponse = await client.GetAsync($"{baseUrl}/v1/orders/{orderId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        Assert.Contains("ReadyForFulfillment", getBody);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_Returns404()
    {
        // BAD: Fresh HttpClient again
        var client = new HttpClient();

        // BAD: Hardcoded URL (fourth occurrence)
        var url = $"http://localhost:5000/v1/orders/{Guid.NewGuid()}";

        // BAD: Duplicated tenant header even though it's not strictly needed for GET
        client.DefaultRequestHeaders.Add("x-kibo-tenant", "tenant-abc-123");

        var response = await client.GetAsync(url);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
