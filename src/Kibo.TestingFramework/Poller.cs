namespace Kibo.TestingFramework;

public static class Poller
{
    public static async Task<T> UntilAsync<T>(
        Func<Task<T>> action,
        Func<T, bool> isMatch,
        TimeSpan timeout,
        TimeSpan interval,
        string timeoutMessage,
        CancellationToken cancellationToken = default)
    {
        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be greater than zero.");
        }

        var startedAt = TimeProvider.System.GetTimestamp();
        T? lastResult = default;

        while (TimeProvider.System.GetElapsedTime(startedAt) <= timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lastResult = await action();
            if (isMatch(lastResult))
            {
                return lastResult;
            }

            await Task.Delay(interval, cancellationToken);
        }

        throw new TimeoutException(
            $"{timeoutMessage} Timed out after {timeout.TotalSeconds:0.###}s while polling every {interval.TotalMilliseconds:0}ms. Last result: {lastResult}");
    }
}
