namespace BereansPath.Api.Diagnostics;

public sealed class RequestLoggingMiddleware(RequestDelegate next, AppLogStore logStore, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/diagnostics"))
        {
            await next(context);
            return;
        }

        var started = DateTime.UtcNow;
        try
        {
            await next(context);
        }
        finally
        {
            var elapsedMs = (DateTime.UtcNow - started).TotalMilliseconds;
            var status = context.Response.StatusCode;
            var line = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString} → {status} ({elapsedMs:0} ms)";
            var level = status >= 500 ? "ERROR" : status >= 400 ? "WARN" : "INFO";
            logStore.Write(level, "HTTP", line);

            if (status >= 400)
            {
                logger.LogWarning("Request completed with {Status}: {Method} {Path}", status, context.Request.Method, context.Request.Path);
            }
        }
    }
}
