using System.Net;
using System.Text;

namespace Kibo.TestingFramework;

public static class ApiResponseAssertions
{
    public static void ShouldHaveStatusCode(this ApiResponse response, HttpStatusCode expectedStatusCode)
    {
        if (response.StatusCode == expectedStatusCode)
        {
            return;
        }

        throw new InvalidOperationException(BuildStatusCodeFailureMessage(response, expectedStatusCode));
    }

    private static string BuildStatusCodeFailureMessage(ApiResponse response, HttpStatusCode expectedStatusCode)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"Expected status: {(int)expectedStatusCode} {expectedStatusCode}");
        builder.AppendLine($"Actual status: {(int)response.StatusCode} {response.StatusCode}");
        builder.AppendLine($"Elapsed: {response.ElapsedMs:0} ms");
        builder.AppendLine($"CorrelationId: {response.CorrelationId}");
        builder.AppendLine();
        builder.AppendLine(response.RequestLog);
        builder.AppendLine(response.ResponseLog);

        return builder.ToString();
    }
}
