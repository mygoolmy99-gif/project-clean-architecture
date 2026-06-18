using System.Reflection;
using HRMS.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRMS.Application.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(
    ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogError(ex, "[HRMS] Unhandled exception for {RequestName}", requestName);

            var error = new Error("UNHANDLED_EXCEPTION", ex.Message);
            var resultType = typeof(TResponse);
            
            if (resultType.IsGenericType)
            {
                var method = resultType.GetMethod(nameof(Result.Failure), BindingFlags.Public | BindingFlags.Static, [typeof(Error)]);
                return (TResponse)method!.Invoke(null, [error])!;
            }
            
            return (TResponse)Result.Failure(error);
        }
    }
}
