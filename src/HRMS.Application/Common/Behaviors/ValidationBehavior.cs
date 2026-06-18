using FluentValidation;
using HRMS.Application.Common;
using MediatR;

namespace HRMS.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Select(f => new Error(f.ErrorCode, f.ErrorMessage, ErrorSeverity.Validation))
            .ToList();

        if (errors.Count != 0)
        {
            return CreateValidationResult(errors);
        }

        return await next();
    }

    private static TResponse CreateValidationResult(List<Error> errors)
    {
        // Fast path: check if TResponse is Result<T>
        if (typeof(TResponse).IsGenericType && 
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var innerType = typeof(TResponse).GetGenericArguments()[0];
            var method = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod(nameof(Result<object>.ValidationError))!;
            
            return (TResponse)method.Invoke(null, [errors])!;
        }
        
        // Fast path: check if TResponse is Result (non-generic)
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.ValidationError(errors);
        }

        throw new InvalidOperationException(
            $"Cannot create validation result for type {typeof(TResponse).Name}. " +
            $"All commands/queries must return Result<T> or Result.");
    }
}
