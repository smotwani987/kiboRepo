using System.Net;
using System.Text;

namespace Kibo.TestingFramework;

public sealed class HttpDiagnostics
{
    public required string Method { get; init; }

    public required Uri RequestUri { get; init; }

    public required string CorrelationId { get; init; }

    public required IReadOnlyDictionary<string, string> RequestHeaders { get; init; }

    public string? RequestBody { get; init; }

    public HttpStatusCode? StatusCode { get; init; }

    public required IReadOnlyDictionary<string, string> ResponseHeaders { get; init; }

    public string? ResponseBody { get; init; }

    public TimeSpan Elapsed { get; init; }

    public double ElapsedMs => Elapsed.TotalMilliseconds;

    public string RequestLog => BuildRequestLog();

    public string ResponseLog => BuildResponseLog();

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.AppendLine("HTTP diagnostics:");
        builder.AppendLine($"CorrelationId: {CorrelationId}");
        builder.AppendLine($"Elapsed: {ElapsedMs:0} ms");
        builder.AppendLine();
        builder.AppendLine(RequestLog);
        builder.AppendLine(ResponseLog);

        return builder.ToString();
    }

    private string BuildRequestLog()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Request:");
        builder.AppendLine($"  Method: {Method}");
        builder.AppendLine($"  Url: {RequestUri}");
        builder.AppendLine("  Headers:");
        AppendHeaders(builder, RequestHeaders);

        if (!string.IsNullOrWhiteSpace(RequestBody))
        {
            builder.AppendLine($"  Body: {RequestBody}");
        }

        return builder.ToString();
    }

    private string BuildResponseLog()
    {
        var builder = new StringBuilder();

        builder.AppendLine("Response:");

        if (StatusCode is not null)
        {
            builder.AppendLine($"  Status: {(int)StatusCode} {StatusCode}");
        }

        builder.AppendLine("  Headers:");
        AppendHeaders(builder, ResponseHeaders);

        if (!string.IsNullOrWhiteSpace(ResponseBody))
        {
            builder.AppendLine($"  Body: {ResponseBody}");
        }

        return builder.ToString();
    }

    private static void AppendHeaders(StringBuilder builder, IReadOnlyDictionary<string, string> headers)
    {
        if (headers.Count == 0)
        {
            builder.AppendLine("    <none>");
            return;
        }

        foreach (var header in headers.OrderBy(header => header.Key, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"    {header.Key}: {header.Value}");
        }
    }
}
