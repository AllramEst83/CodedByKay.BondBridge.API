using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Models;
using CodedByKay.BondBridge.API.Models.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CodedByKay.BondBridge.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        // Removed ApplicationDbContext from constructor injection

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled exception: {ex}");

                // Resolve ApplicationDbContext from the scope
                using (var scope = context.RequestServices.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var logEntry = new Log
                    {
                        Level = "Error",
                        Message = ex.Message,
                        ExceptionMessage = ex.ToString(),
                        StackTrace = ex.StackTrace ?? "No stacktrace available."
                    };
                    dbContext.Logs.Add(logEntry);
                    await dbContext.SaveChangesAsync();
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return context.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = context.Response.StatusCode,
                Message = $"An unexpected error occurred. Please try again later. ({exception.Message})"
            }.ToString());
        }
    }
}
