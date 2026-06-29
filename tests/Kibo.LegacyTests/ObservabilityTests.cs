using System.Net;
using Kibo.TestingFramework;

namespace Kibo.LegacyTests;

public class ObservabilityTests
{
    [Fact]
    public async Task ApiResponse_CapturesElapsedTime()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("timing@example.com")
            .Build();

        var response = await client.CreateOrderAsync(order);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        Assert.InRange(response.ElapsedMs, 0, 5_000);
    }

    [Fact]
    public async Task ApiResponse_ExposesCorrelationId()
    {
        using var client = new KiboApiClient();
        var order = OrderBuilder.New()
            .WithCustomerEmail("correlation@example.com")
            .Build();

        var response = await client.CreateOrderAsync(order);

        response.ShouldHaveStatusCode(HttpStatusCode.Created);
        Assert.False(string.IsNullOrWhiteSpace(response.CorrelationId));
        Assert.Equal(response.CorrelationId, response.Diagnostics.RequestHeaders["x-correlation-id"]);
    }

    [Fact]
    public async Task StatusAssertionFailure_IncludesUsefulDiagnostics()
    {
        using var client = new KiboApiClient();
        var response = await client.GetOrderAsync(Guid.NewGuid());

        var exception = Assert.Throws<InvalidOperationException>(
            () => response.ShouldHaveStatusCode(HttpStatusCode.Created));

        Assert.Contains("Expected status: 201 Created", exception.Message);
        Assert.Contains("Actual status: 404 NotFound", exception.Message);
        Assert.Contains("Elapsed:", exception.Message);
        Assert.Contains("CorrelationId:", exception.Message);
        Assert.Contains("Request:", exception.Message);
        Assert.Contains("Method: GET", exception.Message);
        Assert.Contains("Url: http://localhost:5000/v1/orders/", exception.Message);
        Assert.Contains("x-correlation-id:", exception.Message);
        Assert.Contains("Response:", exception.Message);
        Assert.Contains("Status: 404 NotFound", exception.Message);
        Assert.Contains("Body:", exception.Message);
    }

    [Fact]
    public void Logging_IsOffByDefault_WhenEnvironmentToggleIsUnset()
    {
        var previousValue = Environment.GetEnvironmentVariable("KIBO_API_LOGGING");

        try
        {
            Environment.SetEnvironmentVariable("KIBO_API_LOGGING", null);

            using var client = new KiboApiClient();

            Assert.False(client.LoggingEnabled);
        }
        finally
        {
            Environment.SetEnvironmentVariable("KIBO_API_LOGGING", previousValue);
        }
    }
}
