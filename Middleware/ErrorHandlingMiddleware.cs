using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Othello_API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);

                // 🔹 Handle 404 errors properly
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound && !context.Response.HasStarted)
                {
                    context.Response.Clear();  // Ensure no previous content is sent
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound; // Explicitly set the status
                    var errorResponse = new { message = "Resource not found" };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse));
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                _ => (int)HttpStatusCode.InternalServerError
            };

            response.StatusCode = statusCode;

            var errorResponse = new
            {
                message = exception.Message,
                statusCode
            };

            return response.WriteAsync(JsonSerializer.Serialize(errorResponse));
        }
    }
}
