namespace BereansPath.Api.Diagnostics;

public sealed class AppLoggerProvider(AppLogStore store) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName) => new AppLogger(categoryName, store);

    public void Dispose() { }
}

internal sealed class AppLogger(string category, AppLogStore store) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        if (exception is not null)
        {
            message = $"{message} | {exception.GetType().Name}: {exception.Message}";
        }

        // Skip noisy framework chatter in our ring buffer / file.
        if (category.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) ||
            category.StartsWith("Microsoft.AspNetCore.Routing", StringComparison.Ordinal) ||
            category.StartsWith("Microsoft.AspNetCore.Mvc", StringComparison.Ordinal) ||
            category.StartsWith("Microsoft.AspNetCore.Hosting.Diagnostics", StringComparison.Ordinal) ||
            category.StartsWith("Microsoft.Hosting", StringComparison.Ordinal))
        {
            if (logLevel < LogLevel.Warning) return;
        }

        store.Write(logLevel.ToString().ToUpperInvariant(), ShortCategory(category), message);
    }

    private static string ShortCategory(string category)
    {
        var parts = category.Split('.');
        return parts.Length <= 2 ? category : string.Join('.', parts[^2..]);
    }
}
