using Microsoft.AspNetCore.Http;

namespace BackOfTheHouse.Extensions;

public class CachingMiddleware
{
    private readonly RequestDelegate _next;

    public CachingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add no-cache headers to all responses
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        context.Response.Headers["Pragma"] = "no-cache";
        context.Response.Headers["Expires"] = "0";

        await _next(context);
    }
}