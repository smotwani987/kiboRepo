using System.Net;
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

        var order = OrderBuilder.New()
            .WithCustomerEmail("john.doe@example.com")
            .WithoutLineItems()
            .WithLineItem(LineItemBuilder.New()
                .WithProductCode("SKU-001")
                .WithQuantity(2)
                .WithUnitPrice(29.99m))
            .Build();

        var response = await client.CreateOrderAsync(order);

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but received {(int)response.StatusCode} {response.StatusCode}.{Environment.NewLine}{response.Diagnostics}");
        Assert.Contains("Pending", response.Body);
    }

    [Fact]
    public async Task CreateOrder_WithoutTenantHeader_Returns401()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("no-tenant@example.com")
            .WithoutLineItems()
            .WithLineItem(LineItemBuilder.New()
                .WithProductCode("SKU-999")
                .WithQuantity(1)
                .WithUnitPrice(9.99m))
            .Build();

        var response = await client.CreateOrderAsync(order, includeTenantHeader: false);

        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected 401 Unauthorized but received {(int)response.StatusCode} {response.StatusCode}.{Environment.NewLine}{response.Diagnostics}");
    }

    [Fact]
    public async Task GetOrder_AfterCreation_StatusBecomesReadyForFulfillment()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("status-check@example.com")
            .WithoutLineItems()
            .WithLineItem(LineItemBuilder.New()
                .WithProductCode("SKU-042")
                .WithQuantity(1)
                .WithUnitPrice(49.99m))
            .Build();

        var createResponse = await client.CreateOrderAsync(order);
        Assert.True(
            createResponse.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but received {(int)createResponse.StatusCode} {createResponse.StatusCode}.{Environment.NewLine}{createResponse.Diagnostics}");

        var createdOrder = createResponse.Deserialize<OrderResponse>();
        Assert.NotNull(createdOrder);

        // ============================================================
        // 🐛 THE QA FLAW — Thread.Sleep makes this test brittle & slow.
        //    The API transitions the order after 5 seconds, so this
        //    test just sleeps for 6 seconds and hopes for the best.
        //    In CI/CD this will be flaky and waste pipeline time.
        // ============================================================
        Thread.Sleep(6000);

        var getResponse = await client.GetOrderAsync(createdOrder.Id);
        Assert.True(
            getResponse.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK but received {(int)getResponse.StatusCode} {getResponse.StatusCode}.{Environment.NewLine}{getResponse.Diagnostics}");

        Assert.Contains("ReadyForFulfillment", getResponse.Body);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_Returns404()
    {
        using var client = new KiboApiClient();

        var response = await client.GetOrderAsync(Guid.NewGuid());

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 404 Not Found but received {(int)response.StatusCode} {response.StatusCode}.{Environment.NewLine}{response.Diagnostics}");
    }
}
