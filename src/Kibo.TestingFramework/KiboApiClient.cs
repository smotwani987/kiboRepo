using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Kibo.TestingFramework;

public sealed class KiboApiClient : IDisposable
{
    private const string TenantHeaderName = "x-kibo-tenant";
    private const string CorrelationIdHeaderName = "x-correlation-id";
    private const string BaseUrlEnvironmentVariableName = "KIBO_BASE_URL";
    private const string DefaultBaseUrl = "http://localhost:5000";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _tenantId;

    public KiboApiClient(
        string? baseUrl = null,
        string tenantId = "tenant-abc-123",
        bool? enableLogging = null)
    {
        _tenantId = tenantId;
        LoggingEnabled = enableLogging ?? IsLoggingEnabledFromEnvironment();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(
                ResolveBaseUrl(baseUrl, Environment.GetEnvironmentVariable(BaseUrlEnvironmentVariableName)),
                UriKind.Absolute)
        };
    }

    public bool LoggingEnabled { get; }

    public Task<ApiResponse> GetAsync(string path, bool includeTenantHeader = true)
    {
        var request = CreateRequest(HttpMethod.Get, path, includeTenantHeader);

        return SendAsync(request, requestBody: null);
    }

    public Task<ApiResponse> GetOrderAsync(Guid orderId, bool includeTenantHeader = true)
    {
        return GetAsync($"/v1/orders/{orderId}", includeTenantHeader);
    }

    public Task<ApiResponse> PostAsync(string path, object body, bool includeTenantHeader = true)
    {
        var requestBody = JsonSerializer.Serialize(body, JsonOptions);
        var request = CreateRequest(HttpMethod.Post, path, includeTenantHeader);

        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        return SendAsync(request, requestBody);
    }

    public Task<ApiResponse> CreateOrderAsync(OrderRequest order, bool includeTenantHeader = true)
    {
        return PostAsync("/v1/orders", order, includeTenantHeader);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool includeTenantHeader)
    {
        var request = new HttpRequestMessage(method, path);
        var correlationId = Guid.NewGuid().ToString();

        request.Headers.Add(CorrelationIdHeaderName, correlationId);

        if (includeTenantHeader)
        {
            request.Headers.Add(TenantHeaderName, _tenantId);
        }

        return request;
    }

    private async Task<ApiResponse> SendAsync(HttpRequestMessage request, string? requestBody)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = request.Headers.GetValues(CorrelationIdHeaderName).Single();
        var requestUri = ResolveRequestUri(request);
        var requestHeaders = CaptureHeaders(request);

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        stopwatch.Stop();

        var diagnostics = new HttpDiagnostics
        {
            Method = request.Method.Method,
            RequestUri = requestUri,
            CorrelationId = correlationId,
            RequestHeaders = requestHeaders,
            RequestBody = requestBody,
            StatusCode = response.StatusCode,
            ResponseHeaders = CaptureHeaders(response),
            ResponseBody = responseBody,
            Elapsed = stopwatch.Elapsed
        };

        if (LoggingEnabled)
        {
            Console.WriteLine(diagnostics);
        }

        return new ApiResponse(response.StatusCode, responseBody, diagnostics);
    }

    private static bool IsLoggingEnabledFromEnvironment()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable("KIBO_API_LOGGING"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveBaseUrl(string? explicitBaseUrl, string? environmentBaseUrl)
    {
        if (!string.IsNullOrWhiteSpace(explicitBaseUrl))
        {
            return explicitBaseUrl;
        }

        return string.IsNullOrWhiteSpace(environmentBaseUrl)
            ? DefaultBaseUrl
            : environmentBaseUrl;
    }

    private static IReadOnlyDictionary<string, string> CaptureHeaders(HttpRequestMessage request)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }
        }

        return headers;
    }

    private static IReadOnlyDictionary<string, string> CaptureHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = string.Join(", ", header.Value);
        }

        return headers;
    }

    private Uri ResolveRequestUri(HttpRequestMessage request)
    {
        if (request.RequestUri is null)
        {
            return _httpClient.BaseAddress!;
        }

        if (request.RequestUri.IsAbsoluteUri)
        {
            return request.RequestUri;
        }

        return new Uri(_httpClient.BaseAddress!, request.RequestUri);
    }
}
