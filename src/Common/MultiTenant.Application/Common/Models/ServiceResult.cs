

using System.Collections.Generic;
using System.Linq;

namespace MultiTenant.Application.Common.Models;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public IEnumerable<ServiceError> Errors { get; set; } = Enumerable.Empty<ServiceError>();

    //public static ServiceResult Success() => new ServiceResult { Succeeded = true };
    //public static ServiceResult Failed(params ServiceError[] errors) => new ServiceResult { Succeeded = false, Errors = errors };

    // Generic helpers to match call sites
    public static ServiceResult<T> Success<T>(T data) => ServiceResult<T>.Success(data);
    public static ServiceResult<T> Failed<T>(params ServiceError[] errors) => ServiceResult<T>.Failed(errors);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data) => new ServiceResult<T> { Succeeded = true, Data = data };
    public new static ServiceResult<T> Failed(params ServiceError[] errors) => new ServiceResult<T> { Succeeded = false, Errors = errors };
}
