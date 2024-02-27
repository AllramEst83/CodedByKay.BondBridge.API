using CodedByKay.BondBridge.API.Models;
using Microsoft.Extensions.Options;

namespace CodedByKay.BondBridge.API.Middleware
{
    public class ValidateCustomHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidateCustomHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // Retrieve ApplicationSettings from the scoped services
            var applicationSettings = context.RequestServices.GetRequiredService<IOptions<ApplicationSettings>>().Value;

            if (!context.Request.Headers.TryGetValue("CodedByKay-BondBridge-header", out var customHeaderValue))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Missing custom header.");
                return;
            }

            // Use the CustomHeader value from ApplicationSettings
            if (customHeaderValue != applicationSettings.CustomHeader)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized: Invalid header value.");
                return;
            }

            await _next(context);
        }
    }
}
