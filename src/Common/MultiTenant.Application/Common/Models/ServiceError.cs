namespace MultiTenant.Application.Common.Models;

public class ServiceError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string? Field { get; set; }

    public ServiceError(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }

    public static ServiceError From(string code, string message, string? field = null)
        => new ServiceError(code, message, field);

    public static ServiceError NotFound => new ServiceError("NotFound", "Resource not found.");
    public static ServiceError CustomMessage(string message) => new ServiceError("Custom", message);
}
