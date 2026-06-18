namespace HRMS.API.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("X-XSS-Protection", "1; mode=block");
            headers.TryAdd("Strict-Transport-Security", "max-age=31536000");
            headers.TryAdd("Content-Security-Policy", "default-src 'self'");
            return Task.CompletedTask;
        });

        await next(context);
    }
}
