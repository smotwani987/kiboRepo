using System.Net;
using System.Text;

namespace Kibo.TestingFramework;

public sealed class HttpDiagnostics
{
    public required string Method { get; init; }

    public required Uri RequestUri { get; init; }

    public required string CorrelationId { get; init; }

    public string? RequestBody { get; init; }

    public HttpStatusCode? StatusCode { get; init; }

    public string? ResponseBody { get; init; }

    public TimeSpan Elapsed { get; init; }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine("HTTP diagnostics:");
        builder.AppendLine($"  Method: {Method}");
        builder.AppendLine($"  Url: {RequestUri}");
        builder.AppendLine($"  CorrelationId: {CorrelationId}");
        builder.AppendLine($"  Elapsed: {Elapsed.TotalMilliseconds:0} ms");

        if (StatusCode is not null)
        {
            builder.AppendLine($"  StatusCode: {(int)StatusCode} {StatusCode}");
        }

        if (!string.IsNullOrWhiteSpace(RequestBody))
        {
            builder.AppendLine($"  RequestBody: {RequestBody}");
        }

        if (!string.IsNullOrWhiteSpace(ResponseBody))
        {
            builder.AppendLine($"  ResponseBody: {ResponseBody}");
        }

        return builder.ToString();
    }
}
