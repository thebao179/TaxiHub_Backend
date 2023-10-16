using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string ApiKeyValue = "ApplicationKey";

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the X-Api-Key header is present and has the correct value
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader)
            && apiKeyHeader == ApiKeyValue)
        {
            // If the header is valid, call the next middleware in the pipeline
            await _next(context);
        }
        else
        {
            // If the header is not valid, return a 401 Unauthorized response
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
        }
    }
}