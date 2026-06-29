using System.Net;

namespace Kibo.TestingFramework;

public sealed class ApiResponse
{
    public ApiResponse(HttpStatusCode statusCode, string body, HttpDiagnostics diagnostics)
    {
        StatusCode = statusCode;
        Body = body;
        Diagnostics = diagnostics;
    }

    public HttpStatusCode StatusCode { get; }

    public string Body { get; }

    public HttpDiagnostics Diagnostics { get; }
}
