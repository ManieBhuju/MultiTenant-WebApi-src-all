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
            httpContext.Response.Clear();
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            var error = new MultiTenant.Application.Common.Models.ServiceResult<object>
            {
                Succeeded = false,
                Errors = new[] { new MultiTenant.Application.Common.Models.ServiceError("UnhandledException", ex.Message) }
            };
            var json = System.Text.Json.JsonSerializer.Serialize(error);
            await httpContext.Response.WriteAsync(json);
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