using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Kibo.TestingFramework;

public sealed class KiboApiClient : IDisposable
{
    private const string TenantHeaderName = "x-kibo-tenant";
    private const string CorrelationIdHeaderName = "x-correlation-id";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _tenantId;

    public KiboApiClient(string baseUrl = "http://localhost:5000", string tenantId = "tenant-abc-123")
    {
        _tenantId = tenantId;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute)
        };
    }

    public Task<ApiResponse> GetAsync(string path, bool includeTenantHeader = true)
    {
        var request = CreateRequest(HttpMethod.Get, path, includeTenantHeader);

        return SendAsync(request, requestBody: null);
    }

    public Task<ApiResponse> PostAsync(string path, object body, bool includeTenantHeader = true)
    {
        var requestBody = JsonSerializer.Serialize(body, JsonOptions);
        var request = CreateRequest(HttpMethod.Post, path, includeTenantHeader);

        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        return SendAsync(request, requestBody);
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

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        stopwatch.Stop();

        var diagnostics = new HttpDiagnostics
        {
            Method = request.Method.Method,
            RequestUri = requestUri,
            CorrelationId = correlationId,
            RequestBody = requestBody,
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            Elapsed = stopwatch.Elapsed
        };

        return new ApiResponse(response.StatusCode, responseBody, diagnostics);
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
