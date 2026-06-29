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

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
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

        response.ShouldHaveStatusCode(HttpStatusCode.Unauthorized);
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
        createResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        var createdOrder = createResponse.Deserialize<OrderResponse>();
        Assert.NotNull(createdOrder);

        var getResponse = await Poller.UntilAsync(
            () => client.GetOrderAsync(createdOrder.Id),
            response => response.Body.Contains("ReadyForFulfillment"),
            timeout: TimeSpan.FromSeconds(10),
            interval: TimeSpan.FromMilliseconds(500),
            timeoutMessage: $"Order {createdOrder.Id} did not become ReadyForFulfillment.");

        getResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        Assert.Contains("ReadyForFulfillment", getResponse.Body);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_Returns404()
    {
        using var client = new KiboApiClient();

        var response = await client.GetOrderAsync(Guid.NewGuid());

        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }
}
