using System.Net;
using System.Text.Json;

namespace StudentManagement.Middleware
{
    /// <summary>
    /// Custom middleware that wraps the entire request pipeline in a try/catch.
    /// Any unhandled exception is logged and converted into a consistent response:
    /// - JSON problem-details style payload for API requests (path starts with /api)
    /// - A redirect to the friendly /Home/Error page for regular browser requests
    ///
    /// Registered very early in Program.cs so it can catch exceptions from everything downstream.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred while processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var isApiRequest = context.Request.Path.StartsWithSegments("/api");

            if (isApiRequest)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = new
                {
                    status = 500,
                    message = "An unexpected error occurred while processing your request.",
                    // Only leak exception details in Development, never in Production.
                    detail = _environment.IsDevelopment() ? exception.Message : null,
                    traceId = context.TraceIdentifier
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
            else
            {
                // For browser requests, redirect to the friendly error page.
                context.Response.Redirect("/Home/Error");
            }
        }
    }

    /// <summary>Extension method so Program.cs can register the middleware with a fluent, readable call.</summary>
    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
