using FluentValidation;
using MediatR;
using MultiTenant.Application.Common.Models;

namespace MultiTenant.Application.Common.Behaviours;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            var errors = failures.Select(f => new ServiceError("ValidationFailed", f.ErrorMessage, f.PropertyName)).ToArray();
            var failed = ServiceResult.Failed<object>(errors);
            // try to cast to TResponse
            object result = failed;
            return (TResponse)result!;
        }

        return await next();
    }
}
