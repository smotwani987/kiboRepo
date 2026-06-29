using System.Net;
using System.Text.Json;

namespace Kibo.TestingFramework;

public sealed class ApiResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiResponse(HttpStatusCode statusCode, string body, HttpDiagnostics diagnostics)
    {
        StatusCode = statusCode;
        Body = body;
        Diagnostics = diagnostics;
    }

    public HttpStatusCode StatusCode { get; }

    public string Body { get; }

    public HttpDiagnostics Diagnostics { get; }

    public T? Deserialize<T>()
    {
        return JsonSerializer.Deserialize<T>(Body, JsonOptions);
    }
}
