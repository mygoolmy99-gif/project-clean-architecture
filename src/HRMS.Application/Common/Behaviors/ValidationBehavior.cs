using System.Reflection;
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
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => new Error(f.ErrorCode, f.ErrorMessage, ErrorSeverity.Validation))
            .ToList();

        if (errors.Count != 0)
        {
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType)
            {
                var method = resultType.GetMethod(nameof(Result.ValidationError), BindingFlags.Public | BindingFlags.Static);
                return (TResponse)method!.Invoke(null, [errors])!;
            }
            
            return (TResponse)Result.ValidationError(errors);
        }

        return await next();
    }
}
