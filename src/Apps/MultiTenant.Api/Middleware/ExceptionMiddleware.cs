namespace MultiTenant.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            LogExceptionToFile(ex);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsync("Internal Server Error");
        }
    }
    private void LogExceptionToFile(Exception ex)
    {
        var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "exceptions.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        var logEntry = $"ErrorExceptionLog-{{{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
        File.AppendAllText(logFilePath, logEntry);
    }
}

public static class  ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}