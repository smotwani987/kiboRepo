using System.Net;
using Kibo.TestingFramework;

namespace Kibo.LegacyTests;

public class OrderEdgeCaseTests
{
    [Fact]
    public async Task CreateOrder_WithSqlInjectionTenantHeader_DocumentsSecurityGap()
    {
        const string tenantId = "tenant-abc-123'; DROP TABLE Orders;--";
        using var client = new KiboApiClient(tenantId: tenantId);
        var order = OrderBuilder.New()
            .WithCustomerEmail("sql-tenant@example.com")
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: reject suspicious tenant header. Actual: API accepts and persists it.
        Assert.Equal(tenantId, createdOrder.TenantId);
    }

    [Fact]
    public async Task CreateOrder_WithZeroOrNegativePricing_DocumentsFinancialValidationGap()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("bad-pricing@example.com")
            .WithoutLineItems()
            .WithLineItems(
                LineItemBuilder.New().WithProductCode("FREE-SKU").WithUnitPrice(0m),
                LineItemBuilder.New().WithProductCode("NEGATIVE-SKU").WithUnitPrice(-10.00m))
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: reject zero or negative unitPrice. Actual: API accepts both values.
        Assert.Contains(createdOrder.LineItems, item => item.UnitPrice == 0m);
        Assert.Contains(createdOrder.LineItems, item => item.UnitPrice == -10.00m);
    }

    [Fact]
    public async Task CreateOrder_WithExtremelyLongCustomerEmail_DocumentsValidationGap()
    {
        using var client = new KiboApiClient();
        var customerEmail = $"{new string('a', 5000)}@example.com";
        var order = OrderBuilder.New()
            .WithCustomerEmail(customerEmail)
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: reject unreasonably long customerEmail. Actual: API accepts it unchanged.
        Assert.Equal(customerEmail, createdOrder.CustomerEmail);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyLineItems_DocumentsValidationGap()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("empty-items@example.com")
            .WithoutLineItems()
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: reject empty lineItems. Actual: API accepts and persists the order.
        Assert.Empty(createdOrder.LineItems);
    }

    [Fact]
    public async Task CreateOrder_WithMissingRequiredFields_DocumentsValidationGap()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail(string.Empty)
            .WithoutLineItems()
            .WithLineItem(LineItemBuilder.New()
                .WithProductCode(string.Empty)
                .WithQuantity(-1))
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: reject missing required fields and invalid quantity. Actual: API accepts them.
        Assert.Equal(string.Empty, createdOrder.CustomerEmail);
        Assert.Equal(string.Empty, createdOrder.LineItems.Single().ProductCode);
        Assert.Equal(-1, createdOrder.LineItems.Single().Quantity);
    }

    [Fact]
    public async Task CreateOrder_WithUnicodeAndSpecialCharacters_DocumentsEncodingBehavior()
    {
        const string customerEmail = "qa+unicode.测试@example.com";
        const string productCode = "SKU-特殊-<script>alert('kibo')</script>";
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail(customerEmail)
            .WithoutLineItems()
            .WithLineItem(LineItemBuilder.New().WithProductCode(productCode))
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: define sanitization/encoding rules. Actual: API echoes special characters unchanged.
        Assert.Equal(customerEmail, createdOrder.CustomerEmail);
        Assert.Equal(productCode, createdOrder.LineItems.Single().ProductCode);
    }

    [Fact]
    public async Task CreateOrder_WithOversizedPayload_DocumentsPayloadHandling()
    {
        using var client = new KiboApiClient();
        var lineItems = Enumerable.Range(1, 250)
            .Select(index => LineItemBuilder.New()
                .WithProductCode($"SKU-BULK-{index:000}")
                .Build())
            .ToArray();

        var order = OrderBuilder.New()
            .WithCustomerEmail("oversized-payload@example.com")
            .WithoutLineItems()
            .WithLineItems(lineItems)
            .Build();

        var response = await client.CreateOrderAsync(order);
        var createdOrder = AssertCreated(response);

        // Expected: enforce payload size limits. Actual: API accepts a large line item array.
        Assert.Equal(250, createdOrder.LineItems.Count);
    }

    private static OrderResponse AssertCreated(ApiResponse response)
    {
        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 Created but received {(int)response.StatusCode} {response.StatusCode}.{Environment.NewLine}{response.Diagnostics}");

        var createdOrder = response.Deserialize<OrderResponse>();
        Assert.NotNull(createdOrder);

        return createdOrder;
    }
}
